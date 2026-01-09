// core/router/routes.dart

class Routes {
  // Auth Routes
  static const String login = '/login';
  static const String signup = '/signup';
  static const String otpVerification = '/otp-verification';
  static const String forgotPassword = '/forgot-password';
  static const String resetPasswordOtp = '/reset-password-otp';
  static const String resetPasswordNew = '/reset-password-new';

  // Family Routes
  static const String selectFamily = '/select-family';
  static const String createFamily = '/create-family';
  static const String manageFamilies = '/manage-families';

  // Main Routes
  static const String dashboard = '/dashboard';
  static const String transactions = '/transactions';
  static const String transactionsAdd = '/transactions/add';
  static const String transactionsEdit = '/transactions/edit';

  // Profile & Settings Routes
  static const String profile = '/profile';
  static const String settings = '/settings';

  // Invitations Routes
  static const String invitationsToJoin = '/invitations-to-join';
  static const String invitations = '/invitations';
  static const String invitationsGuest = '/invitations-guest';

  // MyFamily Routes
  static const String myFamily = '/my-family';
}
