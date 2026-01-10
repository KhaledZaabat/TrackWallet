import 'dart:convert';

import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:go_router/go_router.dart';

/// Notification types matching backend enum
enum NotificationType {
  familyInvitation,
  invitationAccepted,
  invitationDeclined,
  invitationCancelled,
  transactionCreated,
}

class NotificationService {
  static final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();

  static const AndroidNotificationChannel _channel = AndroidNotificationChannel(
    'high_importance_channel',
    'High Importance Notifications',
    description: 'Used for important notifications',
    importance: Importance.max,
  );

  /// Global navigator key for navigation from notifications
  static GlobalKey<NavigatorState>? navigatorKey;

  /// Set the navigator key (call this from main.dart)
  static void setNavigatorKey(GlobalKey<NavigatorState> key) {
    navigatorKey = key;
  }

  static Future<void> initialize() async {
    const AndroidInitializationSettings androidSettings =
        AndroidInitializationSettings('@mipmap/ic_launcher');

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
      debugPrint('🔔 Foreground notification received');
      debugPrint('Title: ${message.notification?.title}');
      debugPrint('Body: ${message.notification?.body}');
      debugPrint('Data: ${message.data}');
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
      icon: '@drawable/ic_notification',
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

    if (type == null || navigatorKey?.currentContext == null) {
      debugPrint('⚠️ Cannot navigate: type=$type, context=${navigatorKey?.currentContext}');
      return;
    }

    final context = navigatorKey!.currentContext!;

    switch (type) {
      case 'FamilyInvitation':
        // Navigate to invitations page to see the new invitation
        context.go(Routes.invitations);
        break;

      case 'InvitationAccepted':
        // Navigate to my family page to see the new member
        context.go(Routes.myFamily);
        break;

      case 'InvitationDeclined':
        // Navigate to invitations page to see updated status
        context.go(Routes.invitations);
        break;

      case 'InvitationCancelled':
        // Navigate to invitations page
        context.go(Routes.invitations);
        break;

      case 'TransactionCreated':
        // Navigate to transactions page to see the new transaction
        context.go(Routes.transactions);
        break;

      default:
        debugPrint('⚠️ Unknown notification type: $type');
        // Default to dashboard
        context.go(Routes.dashboard);
        break;
    }
  }
}
