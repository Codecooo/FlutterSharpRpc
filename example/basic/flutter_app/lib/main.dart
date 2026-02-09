import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:codecooo_csharp_rpc/codecooo_csharp_rpc.dart';
import 'types.dart';

late CsharpRpc csharpRpc;

Future<void> main() async {
  /// The path to our C# program.
  /// In release-mode we will publish the C# app to the flutter build path:
  /// "..\flutter_app\build\windows\runner\Release\csharp"
  /// so, we can use the path: "csharp/CsharpApp.exe"
  var pathToCsharpExecutableFile = kReleaseMode
      ? 'csharp/CsharpApp'
      : "..CsharpApp/bin/Release/net10.0/Debug/CsharpApp";

  /// Create and start CsharpRpc instance.
  /// you can create this instance anywhere in your program, but remember to
  /// dispose is by calling 'csharpRpc.dispose()'
  csharpRpc = await CsharpRpc(pathToCsharpExecutableFile).start();

  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: MyHomePage(),
    );
  }
}

class MyHomePage extends StatefulWidget {
  MyHomePage({super.key});

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  @override
  void initState() {
    super.initState();

    // we listen to notification coming from c# server
    csharpRpc.notifications.listen((notif) {
      if (notif.method != 'updateProgress') return;
      updateProgress(notif.params);
    });
  }

  @override
  void dispose() {
    csharpRpc.dispose();
    super.dispose();
  }

  final _textFieldController = TextEditingController();

  final _textNotifController = TextEditingController();

  void _updateTextField(String text) {
    _textFieldController.value = TextEditingValue(text: text);
  }

  void updateProgress(String text) {
    _textNotifController.value = TextEditingValue(
        text: text);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Flutter Csharp RPC Demo'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  child: const Text('GET CURRENT DATE TIME'),
                  onPressed: () async {
                    /// invoke C# method 'GetCurrentDateTime'
                    /// to get the current date time
                    var currentDateTime =
                        await csharpRpc.invoke(method: "GetCurrentDateTime");

                    _updateTextField(currentDateTime);
                  },
                ),
                Padding(
                  padding: const EdgeInsets.all(10),
                  child: ElevatedButton(
                    child: const Text('SUM NUMBERS 2 + 3'),
                    onPressed: () async {
                      /// invoke C# method 'SumNumbers' with the params '[2, 3]'
                      /// to get the summary of 2 + 5
                      var sumNumbersResult = await csharpRpc
                          .invoke<int>(method: "SumNumbers", params: [2, 3]);

                      _updateTextField(sumNumbersResult.toString());
                    },
                  ),
                ),
                ElevatedButton(
                  child: const Text('GET FILES IN CURRENT FOLDER'),
                  onPressed: () async {
                    /// invoke C# method 'GetFilesInFolder' with param of
                    /// 'GetFilesInFolderRequest' instance to get a list of
                    /// files in the current folder
                    var filesResult = await csharpRpc.invoke(
                      method: "GetFilesInFolder",
                      param: GetFilesInFolderRequest(
                          folderPath: Directory.current.path),
                    );
                    var filesInFolder =
                        FilesInFolderResponse.fromJson(filesResult);

                    _updateTextField(filesInFolder.files.toString());
                  },
                ),
              ],
            ),
            SizedBox(
              width: 618,
              child: TextField(
                controller: _textFieldController,
                maxLines: 5,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'C# Response',
                ),
              ),
            ),
            const SizedBox(
              height: 20,
            ),
            SizedBox(
              width: 618,
              child: TextField(
                controller: _textNotifController,
                maxLines: 5,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'C# Notification',
                ),
              ),
            )
          ],
        ),
      ),
    );
  }
}
