// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'FamXpense';

  @override
  String get login => 'Login';

  @override
  String get signup => 'Sign Up';

  @override
  String get logout => 'Logout';

  @override
  String get email => 'Email';

  @override
  String get password => 'Password';

  @override
  String get confirmPassword => 'Confirm Password';

  @override
  String get fullName => 'Full Name';

  @override
  String get forgotPassword => 'Forgot Password?';

  @override
  String get dontHaveAccount => 'Don\'t have an account?';

  @override
  String get alreadyHaveAccount => 'Already have an account?';

  @override
  String get orContinueWith => 'Or continue with';

  @override
  String get signInWithGoogle => 'Sign in with Google';

  @override
  String get createAccount => 'Create Account';

  @override
  String get welcomeBack => 'Welcome back,';

  @override
  String get enterYourEmail => 'Enter your email';

  @override
  String get enterYourPassword => 'Enter your password';

  @override
  String get enterYourName => 'Enter your name';

  @override
  String get invalidEmail => 'Please enter a valid email';

  @override
  String get passwordRequired => 'Password is required';

  @override
  String get passwordTooShort => 'Password must be at least 6 characters';

  @override
  String get passwordsDoNotMatch => 'Passwords do not match';

  @override
  String get nameRequired => 'Name is required';

  @override
  String get goodToSeeYou => 'Good to see you!';

  @override
  String get letsContinue => 'Let\'s continue the journey.';

  @override
  String get or => 'OR';

  @override
  String get needToVerify => 'Need to verify your account?';

  @override
  String get continueText => 'Continue';

  @override
  String get enterEmailToResend =>
      'Enter your email to resend verification code';

  @override
  String get chooseFromGallery => 'Choose from Gallery';

  @override
  String get takePhoto => 'Take Photo';

  @override
  String get removePhoto => 'Remove Photo';

  @override
  String get fullNameRequired => 'Full name is required';

  @override
  String get fullNameTooShort => 'Full name must be at least 2 characters';

  @override
  String get dobRequired => 'Date of birth is required';

  @override
  String get genderRequired => 'Gender is required';

  @override
  String get fillDetails => 'Fill in your details';

  @override
  String get username => 'Username';

  @override
  String get dateOfBirth => 'Date of Birth';

  @override
  String get select => 'Select';

  @override
  String get gender => 'Gender';

  @override
  String get male => 'Male';

  @override
  String get female => 'Female';

  @override
  String get resetPassword => 'Reset Password';

  @override
  String get sendResetLink => 'Send Reset Link';

  @override
  String get enterEmailForReset =>
      'Enter your email to receive a password reset code';

  @override
  String get otpVerification => 'OTP Verification';

  @override
  String get enterOtp => 'Enter the code sent to your email';

  @override
  String get verify => 'Verify';

  @override
  String get resendCode => 'Resend Code';

  @override
  String get newPassword => 'New Password';

  @override
  String get setNewPassword => 'Set New Password';

  @override
  String get verifyAccount => 'Verify Account';

  @override
  String get dashboard => 'Dashboard';

  @override
  String get budgetThisMonth => 'Budget This Month';

  @override
  String get recentTransactions => 'Recent Transactions';

  @override
  String get viewAll => 'View All';

  @override
  String get noTransactionsYet => 'No transactions yet';

  @override
  String get somethingWentWrong => 'Something went wrong';

  @override
  String get retry => 'Retry';

  @override
  String get transactions => 'Transactions';

  @override
  String get addTransaction => 'Add Transaction';

  @override
  String get editTransaction => 'Edit Transaction';

  @override
  String get deleteTransaction => 'Delete Transaction';

  @override
  String get amount => 'Amount';

  @override
  String get title => 'Title';

  @override
  String get category => 'Category';

  @override
  String get date => 'Date';

  @override
  String get description => 'Description';

  @override
  String get income => 'Income';

  @override
  String get expense => 'Expense';

  @override
  String get save => 'Save';

  @override
  String get cancel => 'Cancel';

  @override
  String get delete => 'Delete';

  @override
  String get confirmDelete => 'Are you sure you want to delete this?';

  @override
  String get transactionDeleted => 'Transaction deleted';

  @override
  String get transactionSaved => 'Transaction saved';

  @override
  String get families => 'Families';

  @override
  String get selectFamily => 'Select a Family';

  @override
  String get createFamily => 'Create Family';

  @override
  String get createYourFirstFamily => 'Create Your First Family';

  @override
  String get noFamiliesYet => 'No Families Yet';

  @override
  String get createFamilyDescription =>
      'Create your first family to start managing expenses together.';

  @override
  String get chooseFamily => 'Choose a family to continue';

  @override
  String get familyName => 'Family Name';

  @override
  String get familyBio => 'Family Bio (optional)';

  @override
  String get enterFamilyName => 'Enter family name';

  @override
  String get enterFamilyBio => 'Enter a short description';

  @override
  String get members => 'Members';

  @override
  String get member => 'Member';

  @override
  String get deleteFamily => 'Delete Family';

  @override
  String deleteFamilyConfirm(String familyName) {
    return 'Are you sure you want to permanently delete \"$familyName\"?\n\nThis will remove all members and cancel pending invitations. Transactions will be preserved for historical data.';
  }

  @override
  String get familyDeleted => 'Family deleted';

  @override
  String get changeFamily => 'Change Family';

  @override
  String get myFamily => 'My Family';

  @override
  String get leaveFamily => 'Leave Family';

  @override
  String get leaveFamilyConfirm =>
      'Are you sure you want to leave this family?\n\nYour transactions will remain with the family for historical data.';

  @override
  String get youLeftFamily => 'You have left the family';

  @override
  String get kickMember => 'Remove Member';

  @override
  String get kickMemberConfirm =>
      'Are you sure you want to remove this member from the family?';

  @override
  String get memberRemoved => 'Member removed successfully';

  @override
  String get editFamily => 'Edit Family';

  @override
  String get familyUpdated => 'Family updated successfully';

  @override
  String get parent => 'Parent';

  @override
  String get child => 'Child';

  @override
  String get invitations => 'Invitations';

  @override
  String get receivedInvitations => 'Received Invitations';

  @override
  String get sentInvitations => 'Sent Invitations';

  @override
  String get pendingInvitations => 'Pending Invitations';

  @override
  String get noInvitations => 'No invitations';

  @override
  String get inviteMember => 'Invite Member';

  @override
  String get sendInvitation => 'Send Invitation';

  @override
  String get acceptInvitation => 'Accept';

  @override
  String get declineInvitation => 'Decline';

  @override
  String get cancelInvitation => 'Cancel Invitation';

  @override
  String get invitationSent => 'Invitation sent';

  @override
  String get invitationAccepted => 'Invitation accepted';

  @override
  String get invitationDeclined => 'Invitation declined';

  @override
  String get invitationCancelled => 'Invitation cancelled';

  @override
  String get inviteByEmail => 'Invite by Email';

  @override
  String get enterMemberEmail => 'Enter member\'s email';

  @override
  String get pending => 'Pending';

  @override
  String get accepted => 'Accepted';

  @override
  String get declined => 'Declined';

  @override
  String get expired => 'Expired';

  @override
  String get settings => 'Settings';

  @override
  String get profile => 'Profile';

  @override
  String get editProfile => 'Edit Profile';

  @override
  String get account => 'Account';

  @override
  String get notifications => 'Notifications';

  @override
  String get language => 'Language';

  @override
  String get changePassword => 'Change Password';

  @override
  String get updatePassword => 'Update your account password';

  @override
  String get viewProfile => 'View Profile';

  @override
  String get security => 'Security';

  @override
  String get theme => 'Theme';

  @override
  String get darkMode => 'Dark Mode';

  @override
  String get lightMode => 'Light Mode';

  @override
  String get about => 'About';

  @override
  String get version => 'Version';

  @override
  String get termsOfService => 'Terms of Service';

  @override
  String get privacyPolicy => 'Privacy Policy';

  @override
  String get help => 'Help';

  @override
  String get contactSupport => 'Contact Support';

  @override
  String get logoutConfirm => 'Are you sure you want to logout?';

  @override
  String get loading => 'Loading...';

  @override
  String get error => 'Error';

  @override
  String get success => 'Success';

  @override
  String get warning => 'Warning';

  @override
  String get info => 'Info';

  @override
  String get close => 'Close';

  @override
  String get confirm => 'Confirm';

  @override
  String get yes => 'Yes';

  @override
  String get no => 'No';

  @override
  String get ok => 'OK';

  @override
  String get done => 'Done';

  @override
  String get next => 'Next';

  @override
  String get back => 'Back';

  @override
  String get skip => 'Skip';

  @override
  String get search => 'Search';

  @override
  String get noResults => 'No results found';

  @override
  String get refresh => 'Refresh';

  @override
  String get update => 'Update';

  @override
  String get edit => 'Edit';

  @override
  String get add => 'Add';

  @override
  String get remove => 'Remove';

  @override
  String get today => 'Today';

  @override
  String get yesterday => 'Yesterday';

  @override
  String get thisWeek => 'This Week';

  @override
  String get thisMonth => 'This Month';

  @override
  String get lastMonth => 'Last Month';

  @override
  String get allTime => 'All Time';

  @override
  String get custom => 'Custom';

  @override
  String get from => 'From';

  @override
  String get to => 'To';

  @override
  String get totalIncome => 'Total Income';

  @override
  String get totalExpense => 'Total Expenses';

  @override
  String get balance => 'Balance';

  @override
  String get budget => 'Budget';

  @override
  String get currentBudget => 'Current Budget';

  @override
  String get setBudget => 'Set Budget';

  @override
  String get networkError => 'Network error. Please check your connection.';

  @override
  String get sessionExpired => 'Session expired. Please login again.';

  @override
  String get unknownError => 'An unknown error occurred.';

  @override
  String get tryAgain => 'Try Again';
}
