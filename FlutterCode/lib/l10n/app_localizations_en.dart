// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get settings => 'Settings';

  @override
  String get security => 'Security';

  @override
  String get changePassword => 'Change Password';

  @override
  String get changePasswordSubtitle => 'Update your account password';

  @override
  String get notifications => 'Notifications';

  @override
  String get notificationRequirement =>
      'At least one notification method must be enabled';

  @override
  String get emailNotifications => 'Email Notifications';

  @override
  String get emailNotificationsSubtitle => 'Receive notifications via email';

  @override
  String get pushNotifications => 'Push Notifications';

  @override
  String get pushNotificationsSubtitle => 'Receive push notifications';

  @override
  String get account => 'Account';

  @override
  String get logout => 'Logout';

  @override
  String get logoutSubtitle => 'Sign out of your account';

  @override
  String get logoutConfirmTitle => 'Logout';

  @override
  String get logoutConfirmMessage =>
      'Are you sure you want to logout? You will need to sign in again to access your account.';

  @override
  String get cancel => 'Cancel';

  @override
  String get viewProfile => 'View Profile';

  @override
  String get retry => 'Retry';

  @override
  String get noUserData => 'No user data available';

  @override
  String get currentPassword => 'Current Password';

  @override
  String get newPassword => 'New Password';

  @override
  String get confirmPassword => 'Confirm Password';

  @override
  String get pleaseEnterCurrentPassword => 'Please enter your current password';

  @override
  String get pleaseEnterNewPassword => 'Please enter a new password';

  @override
  String get passwordMinLength => 'Password must be at least 6 characters';

  @override
  String get pleaseConfirmPassword => 'Please confirm your password';

  @override
  String get passwordsDoNotMatch => 'Passwords do not match';

  @override
  String get update => 'Update';

  @override
  String get cannotDisableAllNotifications =>
      'Cannot Disable All Notifications';

  @override
  String get cannotDisableAllNotificationsMessage =>
      'At least one notification method must remain enabled. You cannot disable both email and push notifications.';

  @override
  String get gotIt => 'Got it';

  @override
  String get language => 'Language';

  @override
  String get languageSubtitle => 'Choose your preferred language';

  @override
  String get selectLanguage => 'Select Language';

  @override
  String get english => 'English';

  @override
  String get french => 'French';

  @override
  String get myFamily => 'My Family';

  @override
  String get leaveFamily => 'Leave Family';

  @override
  String get leaveFamilyConfirmMessage =>
      'Are you sure you want to leave this family?\n\nYour transactions will remain with the family for historical data.';

  @override
  String get leave => 'Leave';

  @override
  String get failedToLoadFamily => 'Failed to load family';

  @override
  String get currentBudget => 'Current Budget';

  @override
  String memberCount(num count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count members',
      one: '1 member',
    );
    return '$_temp0';
  }

  @override
  String get addFamilyBio => 'Add family bio';

  @override
  String get editFamily => 'Edit Family';

  @override
  String get familyName => 'Family Name';

  @override
  String get enterFamilyName => 'Enter family name';

  @override
  String get familyNameRequired => 'Family name is required';

  @override
  String get familyNameMinLength => 'Name must be at least 2 characters';

  @override
  String get familyBioOptional => 'Family Bio (Optional)';

  @override
  String get describeFamilyHint => 'Describe your family...';

  @override
  String get saveChanges => 'Save Changes';

  @override
  String get members => 'Members';

  @override
  String get noFamilyMembersYet => 'No family members yet';

  @override
  String get inviteMembersToSeeHere => 'Invite members to see them here';

  @override
  String get parent => 'Parent';

  @override
  String get you => 'You';

  @override
  String get removeMember => 'Remove Member';

  @override
  String removeMemberConfirmMessage(Object memberName) {
    return 'Are you sure you want to remove $memberName from the family?\n\nTheir transactions will remain with the family.';
  }

  @override
  String get remove => 'Remove';

  @override
  String get profile => 'Profile';

  @override
  String get personalInformation => 'Personal Information';

  @override
  String get fullName => 'Full Name';

  @override
  String get fullNameRequired => 'Full name is required';

  @override
  String get fullNameMinLength => 'Full name must be at least 3 characters';

  @override
  String get gender => 'Gender';

  @override
  String get male => 'Male';

  @override
  String get female => 'Female';

  @override
  String get pleaseSelectGender => 'Please select gender';

  @override
  String get dateOfBirth => 'Date of Birth';

  @override
  String get pleaseSelectBirthDate => 'Please select your birth date';

  @override
  String get updateProfile => 'Update Profile';

  @override
  String get cannotUpdateProfileNow => 'Cannot update profile at this time';

  @override
  String get chooseFromGallery => 'Choose from Gallery';

  @override
  String get takePhoto => 'Take a Photo';

  @override
  String get transactions => 'Transactions';

  @override
  String get transaction => 'Transaction';

  @override
  String get unknown => 'Unknown';

  @override
  String filtersActive(num count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count filters active',
      one: '1 filter active',
    );
    return '$_temp0';
  }

  @override
  String get clear => 'Clear';

  @override
  String get somethingWentWrong => 'Oops! Something went wrong';

  @override
  String get tryAgain => 'Try Again';

  @override
  String get noMatchingTransactions => 'No Matching Transactions';

  @override
  String get noTransactionsYet => 'No Transactions Yet';

  @override
  String get adjustFiltersHint =>
      'Try adjusting your filters to see more results';

  @override
  String get startTrackingHint =>
      'Start tracking your expenses by adding your first transaction';

  @override
  String get clearFilters => 'Clear Filters';

  @override
  String get today => 'Today';

  @override
  String get yesterday => 'Yesterday';

  @override
  String get transactionCreated => 'Transaction created successfully';

  @override
  String get transactionUpdated => 'Transaction updated successfully';

  @override
  String get transactionDeleted => 'Transaction deleted successfully';

  @override
  String get deleteTransaction => 'Delete Transaction?';

  @override
  String get deleteTransactionMessage =>
      'This action cannot be undone. The transaction will be permanently deleted.';

  @override
  String get delete => 'Delete';

  @override
  String get editTransaction => 'Edit Transaction';

  @override
  String get newTransaction => 'New Transaction';

  @override
  String get expense => 'Expense';

  @override
  String get income => 'Income';

  @override
  String get amount => 'Amount';

  @override
  String get enterAmount => 'Enter amount';

  @override
  String get invalidAmount => 'Invalid amount';

  @override
  String get titleOptional => 'Title (Optional)';

  @override
  String get whatIsThisFor => 'What is this for?';

  @override
  String get category => 'Category';

  @override
  String get selectCategory => 'Select Category';

  @override
  String get searchCategory => 'Search category';

  @override
  String get noCategoriesFound => 'No categories found';

  @override
  String get tryDifferentKeywords => 'Try searching with different keywords';

  @override
  String get date => 'Date';

  @override
  String get notesOptional => 'Notes (Optional)';

  @override
  String get addAdditionalDetails => 'Add any additional details...';

  @override
  String get updateTransaction => 'Update Transaction';

  @override
  String get saveTransaction => 'Save Transaction';

  @override
  String get pleaseSelectCategory => 'Please select a category';

  @override
  String get amountGreaterThanZero => 'Amount must be greater than zero';

  @override
  String get filterTransactions => 'Filter Transactions';

  @override
  String get transactionType => 'Transaction Type';

  @override
  String get all => 'All';

  @override
  String get categoryGroup => 'Category Group';

  @override
  String get allCategories => 'All Categories';

  @override
  String get amountRange => 'Amount Range';

  @override
  String get min => 'Min';

  @override
  String get max => 'Max';

  @override
  String get createdBy => 'Created By';

  @override
  String get noFamilyMembersFound => 'No family members found';

  @override
  String get allMembers => 'All Members';

  @override
  String get clearAll => 'Clear All';

  @override
  String applyFiltersCount(Object count) {
    return 'Apply Filters ($count)';
  }

  @override
  String get showAll => 'Show All';

  @override
  String get familyInvitations => 'Family Invitations';

  @override
  String get received => 'Received';

  @override
  String get sent => 'Sent';

  @override
  String get sendInvitation => 'Send Invitation';

  @override
  String get noPendingInvitations => 'No pending invitations';

  @override
  String get checkBackLater => 'Check back later for family invitations';

  @override
  String get failedToLoadInvitations => 'Failed to load invitations';

  @override
  String get invitations => 'Invitations';

  @override
  String get families => 'Families';

  @override
  String toLabel(Object email) {
    return 'To: $email';
  }

  @override
  String fromLabel(Object name) {
    return 'From: $name';
  }

  @override
  String joinFamily(Object familyName) {
    return 'Join $familyName';
  }

  @override
  String get roleParent => 'Role: Parent';

  @override
  String get roleMember => 'Role: Member';

  @override
  String get cancelInvite => 'Cancel Invite';

  @override
  String get decline => 'Decline';

  @override
  String get accept => 'Accept';

  @override
  String get sendFamilyInvitation => 'Send Family Invitation';

  @override
  String get emailAddress => 'Email Address';

  @override
  String get emailPlaceholder => 'user@example.com';

  @override
  String get emailRequired => 'Email is required';

  @override
  String get invalidEmail => 'Please enter a valid email address';

  @override
  String get cannotInviteYourself => 'Cannot invite yourself';

  @override
  String get inviteAsParent => 'Invite as family parent';

  @override
  String get parentsCanManage => 'Parents can manage family invitations';

  @override
  String get send => 'Send';

  @override
  String get createFamily => 'Create Family';

  @override
  String get startFamilyJourney => 'Start Your Family Journey';

  @override
  String get createFamilyDescription =>
      'Create a family to manage expenses together';

  @override
  String get familyNameRequired2 => 'Please enter a family name';

  @override
  String get familyNameHint => 'The Smith Family';

  @override
  String get initialBudget => 'Initial Budget';

  @override
  String get enterInitialBudget => 'Please enter an initial budget';

  @override
  String get enterValidNumber => 'Please enter a valid number';

  @override
  String get budgetCannotBeNegative => 'Budget cannot be negative';

  @override
  String get shortFamilyDescription => 'A short description of your family...';

  @override
  String familyCreatedSuccess(Object familyName) {
    return 'Family \"$familyName\" created!';
  }

  @override
  String get createFamilyInfo =>
      'You will automatically become the admin of this family. You can invite other members from the family settings later.';

  @override
  String get selectAFamily => 'Select a Family';

  @override
  String get createYourFirstFamily => 'Create your first family';

  @override
  String get chooseFamily => 'Choose a family to continue';

  @override
  String get noFamiliesYet => 'No Families Yet';

  @override
  String get createFirstFamilyHint =>
      'Create your first family to start managing expenses together.';

  @override
  String get createYourFirstFamilyButton => 'Create Your First Family';

  @override
  String get failedToLoadFamilies => 'Failed to load families';

  @override
  String get deleteFamily => 'Delete Family';

  @override
  String deleteFamilyConfirmMessage(Object familyName) {
    return 'Are you sure you want to permanently delete \"$familyName\"?\n\nThis will remove all members and cancel pending invitations. Transactions will be preserved for historical data.';
  }

  @override
  String get member => 'Member';

  @override
  String get welcomeBack => 'Welcome back,';

  @override
  String get budgetThisMonth => 'Budget This Month';

  @override
  String get recentTransactions => 'Recent Transactions';

  @override
  String get viewAll => 'View All';

  @override
  String get noTransactionsYetDashboard => 'No transactions yet';

  @override
  String get dashboard => 'Dashboard';

  @override
  String get goodToSeeYou => 'Good to see you!';

  @override
  String get letsContinueJourney => 'Let\'s continue the journey.';

  @override
  String get or => 'OR';

  @override
  String get dontHaveAccount => 'Don\'t have an account?';

  @override
  String get signUp => 'Sign Up';

  @override
  String get needToVerifyAccount => 'Need to verify your account?';

  @override
  String get verify => 'Verify';

  @override
  String get verifyAccount => 'Verify Account';

  @override
  String get resendVerificationCode =>
      'Enter your email to resend verification code';

  @override
  String get continueButton => 'Continue';

  @override
  String get resetPassword => 'Reset Password';

  @override
  String get resetPasswordInstructions =>
      'Enter your email to receive password reset instructions';

  @override
  String get sendResetLink => 'Send Reset Link';

  @override
  String get createAccount => 'Create Account';

  @override
  String get fillDetailsToStart => 'Fill in your details to get started';

  @override
  String get username => 'Username';

  @override
  String get usernameHint => 'ExpenseTracker1';

  @override
  String get email => 'Email';

  @override
  String get password => 'Password';

  @override
  String get selectDateHint => 'Select date';

  @override
  String get selectGender => 'Select';

  @override
  String get alreadyHaveAccount => 'Already have an account?';

  @override
  String get logIn => 'Log In';

  @override
  String get removePhoto => 'Remove Photo';

  @override
  String get failedToPickImage => 'Failed to pick image';

  @override
  String get failedToTakePhoto => 'Failed to take photo';

  @override
  String get forgotPassword => 'Forgot Password?';

  @override
  String get forgotPasswordInstructions =>
      'Don\'t worry! Enter your email and we\'ll send you a verification code to reset your password.';

  @override
  String get sendCode => 'Send Code';

  @override
  String get rememberPassword => 'Remember your password?';

  @override
  String get signInWithGoogle => 'Sign in with Google';

  @override
  String get verifyYourEmail => 'Verify Your Email';

  @override
  String get otpSentTo => 'We\'ve sent a 4-digit verification code to';

  @override
  String get accountVerifiedSuccess => 'Account verified successfully!';

  @override
  String get didntReceiveCode => 'Didn\'t receive the code? Resend';

  @override
  String get resending => 'Resending...';

  @override
  String get enterVerificationCode => 'Enter Verification Code';

  @override
  String get weSentCodeTo => 'We\'ve sent a 4-digit code to';

  @override
  String get verifyCode => 'Verify Code';

  @override
  String resendCodeIn(Object seconds) {
    return 'Resend code in $seconds seconds';
  }

  @override
  String get createNewPassword => 'Create New Password';

  @override
  String get newPasswordInstructions =>
      'Your new password must be different from previously used passwords.';

  @override
  String get passwordResetSuccess => 'Password reset successfully!';

  @override
  String get usernameRequired => 'Username is required';

  @override
  String get usernameMinLength => 'Username must be at least 3 characters';

  @override
  String get emailOrUsernameRequired => 'Email or username is required';

  @override
  String get passwordRequired => 'Password is required';

  @override
  String get passwordStrengthError =>
      'Password must be at least 8 characters with uppercase, lowercase, number, and special character';
}
