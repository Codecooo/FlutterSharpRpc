import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'dart:typed_data';
import 'package:collection/collection.dart';
import 'package:csharp_rpc/src/models/rpc_notification.dart';
import 'package:logging/logging.dart';
import 'package:uuid/uuid.dart';
import 'csharp_rpc_request.dart';

/// Manage the communication between dart and C# RPC-server
class CsharpRpc {
  Process? csharpProcess;
  final String _executablePath;
  final Uuid _uuid = const Uuid();
  final List<CsharpRpcRequest> _requests = [];
  late StreamSubscription<List<int>> _stdoutSub;
  late StreamSubscription<List<int>> _stderrSub;
  final _logger = Logger('CsharpRpc');

  final StreamController<RpcNotification> _notificationCtrl =
      StreamController<RpcNotification>.broadcast();

  /// Public getter – callers can `await for` or `listen` to notifications.
  Stream<RpcNotification> get notifications => _notificationCtrl.stream;

  // helper to register a one‑shot callback
  void onNotification(void Function(RpcNotification) handler) {
    notifications.listen(handler);
  }

  /// Accumulates raw bytes coming from the child process.
  final BytesBuilder _incomingBuffer = BytesBuilder();

  /// Cached length of the next JSON payload once we have read the header.
  int? _nextPayloadLength;

  /// Regular expression that extracts the numeric value from a
  /// `Content‑Length: <number>` header (case‑insensitive).
  static final RegExp _contentLengthHeader = RegExp(
    r'Content-Length:\s*(\d+)',
    caseSensitive: false,
    multiLine: true,
  );

  CsharpRpc(this._executablePath);

  /// Start C#-RPC child process.
  Future<CsharpRpc> start() async {
    csharpProcess = await Process.start(_executablePath, []);
    _stdoutSub = csharpProcess!.stdout.listen(_onDataReceived);
    _stderrSub = csharpProcess!.stderr.listen(_onLogReceived);
    return this;
  }

  /// Dispose the C#-RPC child process
  void dispose() {
    _stdoutSub.cancel();
    _stderrSub.cancel();
    csharpProcess?.kill();
    _notificationCtrl.close();
    _requests.clear();
  }

  /// Invoke C#-RPC method by name with (optional) param(s)
  Future<TResult> invoke<TResult>(
      {required String method, List<dynamic>? params, Object? param}) {
    /// create a unique id for the RPC request
    var id = _uuid.v4();

    /// encode json request in RPC format.
    /// jsonrpc version and 'Content-Length' header
    var jsonEncodedBody = jsonEncode({
      "jsonrpc": "2.0",
      "method": method,
      if (params != null || param != null) "params": params ?? [param],
      "id": id
    });
    var contentLengthHeader = 'Content-Length: ${jsonEncodedBody.length}';
    var messagePayload = '$contentLengthHeader\r\n\r\n$jsonEncodedBody';

    /// write ('send') the request to the STDIN stream
    csharpProcess?.stdin.write(messagePayload);

    /// create a CsharpRpcRequest instance for this request
    var csharpRpcRequest = CsharpRpcRequest<TResult>(id);
    _requests.add(csharpRpcRequest);

    return csharpRpcRequest.completer.future;
  }

  /// Called for every chunk that the child writes to STDOUT.
  void _onDataReceived(List<int> chunk) {
    // Append the new bytes to our growing buffer.
    _incomingBuffer.add(chunk);

    while (true) {
      // If we don't yet know the length of the next payload,
      // look for a complete header (`\r\n\r\n`).
      if (_nextPayloadLength == null) {
        final buffered = _incomingBuffer.toBytes();

        // Header terminator (CRLFCRLF) – we search for the first occurrence.
        final headerEnd =
            _indexOfSequence(buffered, [13, 10, 13, 10]); // \r\n\r\n
        if (headerEnd == -1) {
          return;
        }

        // Extract the header bytes (everything before the empty line).
        final headerBytes = buffered.sublist(0, headerEnd);
        final headerString = ascii.decode(headerBytes);

        // Pull the numeric value from the `Content‑Length` header.
        final match = _contentLengthHeader.firstMatch(headerString);
        if (match == null) {
          // Malformed header – abort the whole connection.
          _failAllPending(
              const FormatException('Missing Content‑Length header.'));
          return;
        }

        _nextPayloadLength = int.parse(match.group(1)!);

        // Remove the header (including the terminating CRLFCRLF) from the buffer.
        final afterHeaderStart = headerEnd + 4; // skip the 4 bytes of \r\n\r\n
        final remaining = buffered.sublist(afterHeaderStart);
        _incomingBuffer.clear();
        _incomingBuffer.add(remaining);
      }

      final needed = _nextPayloadLength!;
      final available = _incomingBuffer.length;

      if (available < needed) {
        return;
      }

      // Extract the full json payload
      final fullBuffer = _incomingBuffer.takeBytes();
      final payloadBytes = fullBuffer.sublist(0, needed);
      final leftoverBytes = fullBuffer.sublist(needed);
      _incomingBuffer.clear();
      _incomingBuffer.add(leftoverBytes);

      // Reset for the next iteration.
      _nextPayloadLength = null;

      // Decode the JSON and resolve the matching request.
      _processJsonMessage(utf8.decode(payloadBytes));
      // Loop again – there might be another complete frame already in the buffer.
    }
  }

  int _indexOfSequence(Uint8List data, List<int> seq) {
    // Simple sliding‑window search
    for (int i = 0; i <= data.length - seq.length; i++) {
      bool match = true;
      for (int j = 0; j < seq.length; j++) {
        if (data[i + j] != seq[j]) {
          match = false;
          break;
        }
      }
      if (match) return i;
    }
    return -1; // not found
  }

  void _processJsonMessage(String jsonString) {
    late Map<String, dynamic> map;
    try {
      map = jsonDecode(jsonString) as Map<String, dynamic>;
    } on FormatException catch (e) {
      // Fatal JSON – fail all pending requests and also emit a notification error.
      _failAllPending(e);
      _notificationCtrl.addError(NotificationError(e));
      return;
    }

    // Detect if the content has ID if its not then we treat it as notification
    if (!map.containsKey('id')) {
      final method = map['method'] as String?;
      if (method == null) {
        // No method name – treat as malformed.
        _notificationCtrl
            .addError(NotificationError('Notification without method field'));
        return;
      }

      final dynamic rawParams = map['params']; // could be null, int, List, Map, etc.

      dynamic normalizedParams;

      if (rawParams == null) {
        normalizedParams = null; 
      } else if (rawParams is List) {
        // If the list has exactly one element, unwrap it.
        normalizedParams = (rawParams.length == 1) ? rawParams.first : rawParams;
      } else {
        // Primitive (int, double, String, bool) or a Map – keep as‑is.
        normalizedParams = rawParams;
      }

      final notification = RpcNotification(method, normalizedParams);
      _notificationCtrl.add(notification);
      return;
    }

    final requestId = map['id'];
    final request = _requests.firstWhereOrNull((r) => r.requestId == requestId);

    if (request == null) {
      _logger.warning('Received response for unknown id: $requestId');
      return;
    }

    if (map.containsKey('error')) {
      request.completer.completeError(map['error']);
    } else {
      request.completer.complete(map['result']);
    }

    _requests.remove(request);
  }

  /// write logs from the STDERR stream
  dynamic _onLogReceived(event) {
    // use 'assert' to print logs only if debug mode
    // this is workaround because dart don't have the kDebugMode constant
    assert(() {
      // ignore: avoid_print
      print(utf8.decode(event));
      return true;
    }());
  }

  void _failAllPending(Object error) {
    for (final req in List<CsharpRpcRequest>.from(_requests)) {
      if (!req.completer.isCompleted) {
        req.completer.completeError(error);
      }
    }
    _requests.clear();
  }
}
