import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:firebase_messaging/firebase_messaging.dart';

class NotificationService {
  static final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();

  static const AndroidNotificationChannel _channel = AndroidNotificationChannel(
    'high_importance_channel',
    'High Importance Notifications',
    description: 'Used for important notifications',
    importance: Importance.max,
  );

  static Future<void> initialize() async {
    const AndroidInitializationSettings androidSettings =
        AndroidInitializationSettings('@drawable/app_icon');

    const InitializationSettings settings = InitializationSettings(
      android: androidSettings,
    );

    await _localNotifications.initialize(
      settings,
      onDidReceiveNotificationResponse: _onNotificationTap,
    );

    final AndroidFlutterLocalNotificationsPlugin? androidPlugin =
        _localNotifications.resolvePlatformSpecificImplementation<
            AndroidFlutterLocalNotificationsPlugin>();

    await androidPlugin?.createNotificationChannel(_channel);
  }

  /// Foreground notifications
  static void setupForegroundNotifications() {
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      print('🔔 Foreground notification received');
      print('Title: ${message.notification?.title}');
      print('Body: ${message.notification?.body}');
      print('Data: ${message.data}');
      _showLocalNotification(message);
    });

    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      _handleMessage(message);
    });
  }

  /// App opened from terminated state
  static Future<void> setupInteractedMessage() async {
    final RemoteMessage? initialMessage =
        await FirebaseMessaging.instance.getInitialMessage();

    if (initialMessage != null) {
      _handleMessage(initialMessage);
    }
  }

  static Future<void> _showLocalNotification(RemoteMessage message) async {
    final RemoteNotification? notification = message.notification;
    if (notification == null) return;

    final AndroidNotificationDetails androidDetails =
        AndroidNotificationDetails(
      _channel.id,
      _channel.name,
      channelDescription: _channel.description,
      importance: Importance.max,
      priority: Priority.high,
      icon: '@drawable/app_icon',
    );

    final NotificationDetails details =
        NotificationDetails(android: androidDetails);

    await _localNotifications.show(
      notification.hashCode,
      notification.title,
      notification.body,
      details,
      payload: jsonEncode(message.data),
    );
  }

  static void _onNotificationTap(NotificationResponse response) {
    final String? payload = response.payload;
    if (payload == null) return;

    final Map<String, dynamic> data =
        Map<String, dynamic>.from(jsonDecode(payload));

    _navigateFromNotification(data);
  }

  static void _handleMessage(RemoteMessage message) {
    final Map<String, dynamic> data = Map<String, dynamic>.from(message.data);

    _navigateFromNotification(data);
  }

  static void _navigateFromNotification(Map<String, dynamic> data) {
    final String? type = data['type'];
    final String? notificationId = data['notificationId'];

    debugPrint('🔔 Notification tapped');
    debugPrint('Type: $type');
    debugPrint('NotificationId: $notificationId');

    // TODO:
    // Route by type
    // Example:
    // if (type == 'TransactionCreated') {
    //   navigatorKey.currentState?.pushNamed('/transaction');
    // }
  }
}
