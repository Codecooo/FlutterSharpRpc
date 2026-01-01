// Simple model class for RpcNotification received from server
class RpcNotification {
  final String method;
  final dynamic params; 

  RpcNotification(this.method, this.params);
}

// error wrapper for malformed notifications
class NotificationError implements Exception {
  final Object cause;
  NotificationError(this.cause);
}