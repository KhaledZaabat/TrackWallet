// presentation/settings/pages/settings_page.dart

import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/features/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/Settings/Cubits/settings_cubit.dart';
import 'package:famxpense/features/Settings/Cubits/settings_state.dart';
import 'package:famxpense/features/Settings/Cubits/locale_cubit.dart';
import 'package:famxpense/l10n/app_localizations.dart';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';

class SettingsPage extends StatefulWidget {
  const SettingsPage({super.key});

  @override
  State<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> {
  bool _isFamilySelected = false; // Initialize _isFamilySelected

  @override
  void initState() {
    super.initState();
    // Check if family is selected
    getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
      setState(() {
        _isFamilySelected = familyId != null && familyId.isNotEmpty;
      });
    });

    // Load settings - using post frame callback to ensure context is available
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        final cubit = context.read<SettingsCubit>();
        final state = cubit.state;

        if (state is! SettingsLoaded && state is! SettingsLoading) {
          cubit.loadSettings();
        }
        cubit.loadSettings();
      }
    });
  }

  void _onStateChanged(BuildContext context, SettingsState state) {
    if (state is SettingsUpdateSuccess) {
      _showSnackBar(state.message, isError: false);
    }

    if (state is SettingsError && state.user != null) {
      _showSnackBar(state.error, isError: true);
    }
  }

  void _showSnackBar(String message, {bool isError = false}) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? AppColors.error : AppColors.success,
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      ),
    );
  }

  Future<void> _showChangePasswordDialog() async {
    final l10n = AppLocalizations.of(context)!;
    final currentPasswordController = TextEditingController();
    final newPasswordController = TextEditingController();
    final confirmPasswordController = TextEditingController();
    final formKey = GlobalKey<FormState>();

    final result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (dialogContext) => _PasswordDialog(
        formKey: formKey,
        currentPasswordController: currentPasswordController,
        newPasswordController: newPasswordController,
        confirmPasswordController: confirmPasswordController,
      ),
    );

    if (result == true && mounted) {
      context.read<SettingsCubit>().updatePassword(
            currentPassword: currentPasswordController.text,
            newPassword: newPasswordController.text,
          );
    }

    await Future.delayed(const Duration(milliseconds: 100));
    currentPasswordController.dispose();
    newPasswordController.dispose();
    confirmPasswordController.dispose();
  }

  Future<void> _showLogoutDialog() async {
    final l10n = AppLocalizations.of(context)!;
    final confirmed = await showDialog<bool>(
      context: context,
      barrierDismissible: true,
      builder: (dialogContext) => AlertDialog(
        backgroundColor: AppColors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
        title: Row(
          children: [
            Icon(
              Icons.logout_rounded,
              color: AppColors.error,
              size: 28,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                l10n.logoutConfirmTitle,
                style: TextStyle(
                  fontWeight: FontWeight.w700,
                  fontSize: 20,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          l10n.logoutConfirmMessage,
          style: TextStyle(
            fontSize: 15,
            color: AppColors.textSecondary,
            fontFamily: GoogleFonts.inter().fontFamily,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext, false),
            child: Text(
              l10n.cancel,
              style: TextStyle(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
                fontSize: 15,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(dialogContext, true),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10),
              ),
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            ),
            child: Text(
              l10n.logout,
              style: TextStyle(
                fontWeight: FontWeight.w600,
                fontSize: 15,
              ),
            ),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      // Call logout - the AuthCubit will emit AuthUnauthenticated
      // and the GoRouter's refreshListenable will handle navigation to login
      await getIt<AuthCubit>().logout();
      
      // Use a post-frame callback to navigate after the current frame completes
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted) {
          context.go(Routes.login);
        }
      });
    }
  }

  Future<void> _showLanguageDialog() async {
    final l10n = AppLocalizations.of(context)!;
    final localeCubit = context.read<LocaleCubit>();
    final currentLocale = localeCubit.currentLocale;

    final selectedLocale = await showDialog<Locale>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        backgroundColor: AppColors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
        title: Text(
          l10n.selectLanguage,
          style: TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 20,
            fontFamily: GoogleFonts.inter().fontFamily,
          ),
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _buildLanguageOption(
              context: dialogContext,
              locale: const Locale('en'),
              name: l10n.english,
              flag: '🇬🇧',
              isSelected: currentLocale.languageCode == 'en',
            ),
            const SizedBox(height: 8),
            _buildLanguageOption(
              context: dialogContext,
              locale: const Locale('fr'),
              name: l10n.french,
              flag: '🇫🇷',
              isSelected: currentLocale.languageCode == 'fr',
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: Text(
              l10n.cancel,
              style: TextStyle(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );

    if (selectedLocale != null && mounted) {
      localeCubit.setLocale(selectedLocale);
    }
  }

  Widget _buildLanguageOption({
    required BuildContext context,
    required Locale locale,
    required String name,
    required String flag,
    required bool isSelected,
  }) {
    return InkWell(
      onTap: () => Navigator.pop(context, locale),
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.primary.withOpacity(0.1) : Colors.transparent,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.border,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: Row(
          children: [
            Text(flag, style: const TextStyle(fontSize: 24)),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                name,
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                  fontFamily: GoogleFonts.inter().fontFamily,
                  color: isSelected ? AppColors.primary : AppColors.textPrimary,
                ),
              ),
            ),
            if (isSelected)
              Icon(
                Icons.check_circle,
                color: AppColors.primary,
                size: 24,
              ),
          ],
        ),
      ),
    );
  }

  // ✅ Validation: Check if trying to disable both notifications
  void _handleNotificationToggle({
    required bool newEmailValue,
    required bool currentPushValue,
    required bool isEmailToggle,
  }) {
    final l10n = AppLocalizations.of(context)!;
    // If trying to disable both notifications
    if (!newEmailValue && !currentPushValue) {
      _showValidationDialog(
        l10n.cannotDisableAllNotifications,
        l10n.cannotDisableAllNotificationsMessage,
      );
      return;
    }

    // Proceed with update
    context.read<SettingsCubit>().updateNotificationPreferences(
          emailNotifications: newEmailValue,
          pushNotifications: currentPushValue,
        );
  }

  void _showValidationDialog(String title, String message) {
    final l10n = AppLocalizations.of(context)!;
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: AppColors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
        title: Row(
          children: [
            Icon(
              Icons.warning_amber_rounded,
              color: AppColors.textPrimary,
              size: 28,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                title,
                style: TextStyle(
                  fontFamily: GoogleFonts.inter().fontFamily,
                  fontWeight: FontWeight.w700,
                  fontSize: 18,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          message,
          style: TextStyle(
            fontSize: 15,
            color: AppColors.textSecondary,
            fontFamily: GoogleFonts.inter().fontFamily,
          ),
        ),
        actions: [
          ElevatedButton(
            onPressed: () => Navigator.pop(context),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10),
              ),
            ),
            child: Text(l10n.gotIt),
          ),
        ],
      ),
    );
  }

  dynamic _getUserFromState(SettingsState state) {
    if (state is SettingsLoaded) {
      return state.user;
    } else if (state is SettingsUpdating) {
      return state.user;
    } else if (state is SettingsUpdateSuccess) {
      return state.user;
    } else if (state is SettingsError) {
      return state.user;
    }
    return null;
  }

  String _getCurrentLanguageName(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final locale = context.read<LocaleCubit>().currentLocale;
    return locale.languageCode == 'fr' ? l10n.french : l10n.english;
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: BlocConsumer<SettingsCubit, SettingsState>(
          listener: _onStateChanged,
          builder: (context, state) {
            if (state is SettingsLoading) {
              return const Center(child: CircularProgressIndicator());
            }

            if (state is SettingsError && state.user == null) {
              return Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.error_outline, size: 48, color: AppColors.error),
                    const SizedBox(height: 16),
                    Text(
                      state.error,
                      textAlign: TextAlign.center,
                      style: const TextStyle(fontSize: 16),
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      onPressed: () =>
                          context.read<SettingsCubit>().loadSettings(),
                      icon: const Icon(Icons.refresh),
                      label: Text(l10n.retry),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                      ),
                    ),
                  ],
                ),
              );
            }

            final user = _getUserFromState(state);

            if (user == null) {
              return Center(child: Text(l10n.noUserData));
            }

            final isUpdating = state is SettingsUpdating;

            return SingleChildScrollView(
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 22, vertical: 28),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Header
                    Text(
                      l10n.settings,
                      style: TextStyle(
                        fontSize: 32,
                        fontFamily: GoogleFonts.inter().fontFamily,
                        fontWeight: FontWeight.w700,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    const SizedBox(height: 32),

                    // User Info Card
                    _buildUserInfoCard(user),
                    const SizedBox(height: 24),

                    // View Profile Button
                    _buildProfileButton(context, l10n),
                    const SizedBox(height: 32),

                    // Language Section
                    _sectionHeader(l10n.language),
                    const SizedBox(height: 12),
                    BlocBuilder<LocaleCubit, LocaleState>(
                      builder: (context, localeState) {
                        return _buildSettingTile(
                          icon: Icons.language,
                          title: l10n.language,
                          subtitle: _getCurrentLanguageName(context),
                          onTap: isUpdating ? null : _showLanguageDialog,
                        );
                      },
                    ),
                    const SizedBox(height: 32),

                    // Security Section
                    _sectionHeader(l10n.security),
                    const SizedBox(height: 12),
                    _buildSettingTile(
                      icon: Icons.lock_outline,
                      title: l10n.changePassword,
                      subtitle: l10n.changePasswordSubtitle,
                      onTap: isUpdating ? null : _showChangePasswordDialog,
                    ),
                    const SizedBox(height: 32),

                    // Notifications Section
                    _sectionHeader(l10n.notifications),
                    const SizedBox(height: 8),

                    // ✅ Important Note
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 10,
                      ),
                      decoration: BoxDecoration(
                        color: AppColors.surface,
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(
                          color: AppColors.border,
                          width: 1,
                        ),
                      ),
                      child: Row(
                        children: [
                          Icon(
                            Icons.info_outline,
                            color: AppColors.textPrimary,
                            size: 20,
                          ),
                          const SizedBox(width: 10),
                          Expanded(
                            child: Text(
                              l10n.notificationRequirement,
                              style: TextStyle(
                                fontSize: 12,
                                color: AppColors.textPrimary,
                                fontFamily: GoogleFonts.inter().fontFamily,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 12),

                    _buildNotificationTile(
                      icon: Icons.email_outlined,
                      title: l10n.emailNotifications,
                      subtitle: l10n.emailNotificationsSubtitle,
                      value: user.emailNotifications,
                      onChanged: isUpdating
                          ? null
                          : (value) {
                              _handleNotificationToggle(
                                newEmailValue: value,
                                currentPushValue: user.pushNotifications,
                                isEmailToggle: true,
                              );
                            },
                    ),
                    const SizedBox(height: 12),
                    _buildNotificationTile(
                      icon: Icons.notifications_outlined,
                      title: l10n.pushNotifications,
                      subtitle: l10n.pushNotificationsSubtitle,
                      value: user.pushNotifications,
                      onChanged: isUpdating
                          ? null
                          : (value) {
                              // Check if trying to disable both
                              if (!value && !user.emailNotifications) {
                                _showValidationDialog(
                                  l10n.cannotDisableAllNotifications,
                                  l10n.cannotDisableAllNotificationsMessage,
                                );
                                return;
                              }

                              // Proceed with update
                              context
                                  .read<SettingsCubit>()
                                  .updateNotificationPreferences(
                                    emailNotifications: user.emailNotifications,
                                    pushNotifications: value,
                                  );
                            },
                    ),
                    const SizedBox(height: 32),

                    // Account Section
                    _sectionHeader(l10n.account),
                    const SizedBox(height: 12),
                    _buildSettingTile(
                      icon: Icons.logout_rounded,
                      title: l10n.logout,
                      subtitle: l10n.logoutSubtitle,
                      onTap: isUpdating ? null : _showLogoutDialog,
                      iconColor: AppColors.error,
                      isDestructive: true,
                    ),
                    const SizedBox(height: 32),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  Widget _buildUserInfoCard(user) {
    final hasProfilePicture = user.profileImageUrl.isNotEmpty;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 32,
            backgroundColor: AppColors.border,
            child: hasProfilePicture
                ? ClipOval(
                    child: Image.network(
                      user.profileImageUrl,
                      width: 64,
                      height: 64,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => const Icon(
                        Icons.person,
                        size: 32,
                        color: AppColors.lightGrey,
                      ),
                    ),
                  )
                : const Icon(
                    Icons.person,
                    size: 32,
                    color: AppColors.lightGrey,
                  ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  user.fullName,
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.w700,
                    fontFamily: GoogleFonts.inter().fontFamily,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  user.email,
                  style: TextStyle(
                    fontSize: 14,
                    color: AppColors.textSecondary,
                    fontFamily: GoogleFonts.inter().fontFamily,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProfileButton(BuildContext context, AppLocalizations l10n) {
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.primary,
          padding: const EdgeInsets.symmetric(vertical: 14),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          elevation: 0,
        ),
        onPressed: () {
          context.push(Routes.profile);
        },
        child: Text(
          l10n.viewProfile,
          style: TextStyle(
            color: Colors.white,
            fontSize: 16,
            fontFamily: GoogleFonts.inter().fontFamily,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }

  Widget _sectionHeader(String title) {
    return Text(
      title,
      style: TextStyle(
        color: AppColors.textPrimary,
        fontSize: 16,
        fontFamily: GoogleFonts.inter().fontFamily,
        fontWeight: FontWeight.w700,
      ),
    );
  }

  Widget _buildSettingTile({
    required IconData icon,
    required String title,
    required String subtitle,
    VoidCallback? onTap,
    Color? iconColor,
    bool isDestructive = false,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: Container(
          padding: const EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: isDestructive
                ? AppColors.error.withOpacity(0.1)
                : AppColors.primary.withOpacity(0.1),
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(
            icon,
            color: iconColor ?? AppColors.primary,
            size: 24,
          ),
        ),
        title: Text(
          title,
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w600,
            fontFamily: GoogleFonts.inter().fontFamily,
            color: isDestructive ? AppColors.error : null,
          ),
        ),
        subtitle: Text(
          subtitle,
          style: TextStyle(
            fontSize: 13,
            color: AppColors.textSecondary,
            fontFamily: GoogleFonts.inter().fontFamily,
          ),
        ),
        trailing: Icon(
          Icons.arrow_forward_ios,
          size: 16,
          color: AppColors.lightGrey,
        ),
        onTap: onTap,
      ),
    );
  }

  Widget _buildNotificationTile({
    required IconData icon,
    required String title,
    required String subtitle,
    required bool value,
    required ValueChanged<bool>? onChanged,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: SwitchListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        secondary: Container(
          padding: const EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: AppColors.primary.withOpacity(0.1),
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(
            icon,
            color: AppColors.primary,
            size: 24,
          ),
        ),
        title: Text(
          title,
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w600,
            fontFamily: GoogleFonts.inter().fontFamily,
          ),
        ),
        subtitle: Text(
          subtitle,
          style: TextStyle(
            fontSize: 13,
            color: AppColors.textSecondary,
            fontFamily: GoogleFonts.inter().fontFamily,
          ),
        ),
        value: value,
        onChanged: onChanged,
        activeColor: AppColors.primary,
      ),
    );
  }
}

// Password Dialog Widget
class _PasswordDialog extends StatefulWidget {
  final GlobalKey<FormState> formKey;
  final TextEditingController currentPasswordController;
  final TextEditingController newPasswordController;
  final TextEditingController confirmPasswordController;

  const _PasswordDialog({
    required this.formKey,
    required this.currentPasswordController,
    required this.newPasswordController,
    required this.confirmPasswordController,
  });

  @override
  State<_PasswordDialog> createState() => _PasswordDialogState();
}

class _PasswordDialogState extends State<_PasswordDialog> {
  bool obscureCurrentPassword = true;
  bool obscureNewPassword = true;
  bool obscureConfirmPassword = true;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return AlertDialog(
      backgroundColor: AppColors.surface,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      title: Text(
        l10n.changePassword,
        style: TextStyle(
          fontFamily: GoogleFonts.inter().fontFamily,
          fontWeight: FontWeight.w700,
          fontSize: 20,
        ),
      ),
      content: Form(
        key: widget.formKey,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: widget.currentPasswordController,
                obscureText: obscureCurrentPassword,
                decoration: InputDecoration(
                  labelText: l10n.currentPassword,
                  prefixIcon: const Icon(Icons.lock_outline, size: 20),
                  suffixIcon: IconButton(
                    icon: Icon(
                      obscureCurrentPassword
                          ? Icons.visibility_off
                          : Icons.visibility,
                      size: 20,
                    ),
                    onPressed: () {
                      setState(() {
                        obscureCurrentPassword = !obscureCurrentPassword;
                      });
                    },
                  ),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(
                      color: AppColors.primary,
                      width: 2,
                    ),
                  ),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return l10n.pleaseEnterCurrentPassword;
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: widget.newPasswordController,
                obscureText: obscureNewPassword,
                decoration: InputDecoration(
                  labelText: l10n.newPassword,
                  prefixIcon: const Icon(Icons.lock_outline, size: 20),
                  suffixIcon: IconButton(
                    icon: Icon(
                      obscureNewPassword
                          ? Icons.visibility_off
                          : Icons.visibility,
                      size: 20,
                    ),
                    onPressed: () {
                      setState(() {
                        obscureNewPassword = !obscureNewPassword;
                      });
                    },
                  ),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(
                      color: AppColors.primary,
                      width: 2,
                    ),
                  ),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return l10n.pleaseEnterNewPassword;
                  }
                  if (value.length < 8) {
                    return l10n.passwordMinLength;
                  }
                  final hasUpperCase = value.contains(RegExp(r'[A-Z]'));
                  final hasLowerCase = value.contains(RegExp(r'[a-z]'));
                  final hasDigit = value.contains(RegExp(r'[0-9]'));
                  final hasSpecialChar =
                      value.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>]'));

                  if (!hasUpperCase ||
                      !hasLowerCase ||
                      !hasDigit ||
                      !hasSpecialChar) {
                    return 'Password must include upper, lower, digit & special char';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: widget.confirmPasswordController,
                obscureText: obscureConfirmPassword,
                decoration: InputDecoration(
                  labelText: l10n.confirmPassword,
                  prefixIcon: const Icon(Icons.lock_outline, size: 20),
                  suffixIcon: IconButton(
                    icon: Icon(
                      obscureConfirmPassword
                          ? Icons.visibility_off
                          : Icons.visibility,
                      size: 20,
                    ),
                    onPressed: () {
                      setState(() {
                        obscureConfirmPassword = !obscureConfirmPassword;
                      });
                    },
                  ),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(
                      color: AppColors.primary,
                      width: 2,
                    ),
                  ),
                ),
                validator: (value) {
                  if (value == null || value.isEmpty) {
                    return l10n.pleaseConfirmPassword;
                  }
                  if (value != widget.newPasswordController.text) {
                    return l10n.passwordsDoNotMatch;
                  }
                  return null;
                },
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: Text(
            l10n.cancel,
            style: TextStyle(
              color: AppColors.textSecondary,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
        ElevatedButton(
          onPressed: () {
            if (widget.formKey.currentState!.validate()) {
              Navigator.pop(context, true);
            }
          },
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),
          child: Text(l10n.update),
        ),
      ],
    );
  }
}
