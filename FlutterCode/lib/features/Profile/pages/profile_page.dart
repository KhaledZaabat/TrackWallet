// presentation/profile/pages/profile_page.dart

import 'dart:io';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/domain/entities/user.dart';
import 'package:famxpense/features/Profile/Cubits/profile_cubit.dart';
import 'package:famxpense/features/Profile/Cubits/profile_state.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:image_picker/image_picker.dart';
import 'package:intl/intl.dart';

class ProfilePage extends StatefulWidget {
  const ProfilePage({super.key});

  @override
  State<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends State<ProfilePage> {
  final _formKey = GlobalKey<FormState>();
  final _fullNameController = TextEditingController();
  final _dobController = TextEditingController();
  final _imagePicker = ImagePicker();

  DateTime? _selectedDob;
  String? _selectedGender;
  File? _selectedImage;
  String? _loadedUserId;

  @override
  void initState() {
    super.initState();
    final cubit = context.read<ProfileCubit>();
    final state = cubit.state;

    if (state is ProfileLoaded) {
      _hydrateForm(state.user);
    } else if (state is! ProfileLoading) {
      cubit.loadProfile();
    }
  }

  @override
  void dispose() {
    _fullNameController.dispose();
    _dobController.dispose();
    super.dispose();
  }

  void _hydrateForm(User user) {
    if (!mounted) return;

    setState(() {
      _loadedUserId = user.id;
      _fullNameController.text = user.fullName;
      _selectedDob = user.birthDate;
      _selectedGender = user.isMale ? 'Male' : 'Female';
      _dobController.text = DateFormat('yyyy-MM-dd').format(user.birthDate);
    });
  }

  Future<void> _selectDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDob ?? DateTime(2000),
      firstDate: DateTime(1900),
      lastDate: DateTime.now(),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: ColorScheme.light(
              primary: AppColors.primary,
              onPrimary: Colors.white,
              onSurface: AppColors.textPrimary,
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() {
        _selectedDob = picked;
        _dobController.text = DateFormat('yyyy-MM-dd').format(picked);
      });
    }
  }

  Future<void> _pickImage() async {
    final l10n = AppLocalizations.of(context)!;
    final source = await showModalBottomSheet<ImageSource>(
      context: context,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading:
                  const Icon(Icons.photo_library, color: AppColors.primary),
              title: Text(l10n.chooseFromGallery),
              onTap: () => Navigator.pop(context, ImageSource.gallery),
            ),
            ListTile(
              leading: const Icon(Icons.camera_alt, color: AppColors.primary),
              title: Text(l10n.takePhoto),
              onTap: () => Navigator.pop(context, ImageSource.camera),
            ),
          ],
        ),
      ),
    );

    if (source != null) {
      final pickedFile = await _imagePicker.pickImage(
        source: source,
        maxWidth: 800,
        maxHeight: 800,
        imageQuality: 85,
      );

      if (pickedFile != null) {
        setState(() {
          _selectedImage = File(pickedFile.path);
        });
      }
    }
  }

  void _onSubmit() {
    final l10n = AppLocalizations.of(context)!;
    FocusScope.of(context).unfocus();

    if (!_formKey.currentState!.validate()) return;

    final cubit = context.read<ProfileCubit>();
    final state = cubit.state;

    if (state is! ProfileLoaded && state is! ProfileError) {
      _showSnackBar(l10n.cannotUpdateProfileNow);
      return;
    }

    cubit.updateProfile(
      fullName: _fullNameController.text.trim(),
      birthDate: _selectedDob!,
      isMale: _selectedGender == 'Male',
      profileImage: _selectedImage,
    );
  }

  void _onStateChanged(BuildContext context, ProfileState state) {
    if (state is ProfileLoaded) {
      if (state.user.id != _loadedUserId) {
        _hydrateForm(state.user);
      }
    }

    if (state is ProfileUpdateSuccess) {
      _showSnackBar(state.message, isError: false);
      setState(() {
        _selectedImage = null;
      });
    }

    if (state is ProfileError && state.user != null) {
      _showSnackBar(state.error, isError: true);
    }
  }

  void _showSnackBar(String message, {bool isError = false}) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? Colors.red : Colors.green,
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      ),
    );
  }

  String? _validateFullName(String? value) {
    final l10n = AppLocalizations.of(context)!;
    if (value == null || value.trim().isEmpty) {
      return l10n.fullNameRequired;
    }
    if (value.trim().length < 3) {
      return l10n.fullNameMinLength;
    }
    return null;
  }

  String? _validateDob(String? _) {
    final l10n = AppLocalizations.of(context)!;
    if (_selectedDob == null) {
      return l10n.pleaseSelectBirthDate;
    }
    return null;
  }

  String? _validateGender(String? value) {
    final l10n = AppLocalizations.of(context)!;
    if (value == null || value.isEmpty) {
      return l10n.pleaseSelectGender;
    }
    return null;
  }

  // ✅ FIXED: Get user from current state properly
  User? _getUserFromState(ProfileState state) {
    if (state is ProfileLoaded) {
      return state.user;
    } else if (state is ProfileUpdating) {
      return state.user;
    } else if (state is ProfileUpdateSuccess) {
      return state.user;
    } else if (state is ProfileError) {
      return state.user;
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: BlocConsumer<ProfileCubit, ProfileState>(
          listener: _onStateChanged,
          builder: (context, state) {
            if (state is ProfileLoading) {
              return const Center(child: CircularProgressIndicator());
            }

            if (state is ProfileError && state.user == null) {
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
                          context.read<ProfileCubit>().loadProfile(),
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

            final isUpdating = state is ProfileUpdating;
            final hasProfilePicture = user.profileImageUrl.isNotEmpty;

            return SingleChildScrollView(
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 22, vertical: 32),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Header
                      Row(
                        children: [
                          GestureDetector(
                            onTap: () => context.pop(),
                            child: Icon(
                              Icons.arrow_back,
                              size: 24,
                              color: AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(width: 12),
                          Text(
                            l10n.profile,
                            style: TextStyle(
                              fontSize: 32,
                              fontWeight: FontWeight.w700,
                              fontFamily: GoogleFonts.inter().fontFamily,
                              color: AppColors.textPrimary,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 32),

                      // Profile Picture
                      Center(
                        child: Stack(
                          children: [
                            GestureDetector(
                              onTap: isUpdating ? null : _pickImage,
                              child: CircleAvatar(
                                radius: 60,
                                backgroundColor: AppColors.border,
                                child: _selectedImage != null
                                    ? ClipOval(
                                        child: Image.file(
                                          _selectedImage!,
                                          width: 120,
                                          height: 120,
                                          fit: BoxFit.cover,
                                        ),
                                      )
                                    : hasProfilePicture
                                        ? ClipOval(
                                            child: Image.network(
                                              user.profileImageUrl,
                                              width: 120,
                                              height: 120,
                                              fit: BoxFit.cover,
                                              errorBuilder: (_, __, ___) =>
                                                  _avatarFallback(),
                                            ),
                                          )
                                        : _avatarFallback(),
                              ),
                            ),
                            Positioned(
                              bottom: 0,
                              right: 0,
                              child: GestureDetector(
                                onTap: isUpdating ? null : _pickImage,
                                child: Container(
                                  padding: const EdgeInsets.all(8),
                                  decoration: const BoxDecoration(
                                    color: AppColors.primary,
                                    shape: BoxShape.circle,
                                  ),
                                  child: const Icon(
                                    Icons.camera_alt,
                                    color: Colors.white,
                                    size: 20,
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 12),

                      // Email (read-only)
                      Center(
                        child: Text(
                          user.email,
                          style: TextStyle(
                            fontSize: 16,
                            color: AppColors.textSecondary,
                            fontFamily: GoogleFonts.inter().fontFamily,
                          ),
                        ),
                      ),
                      const SizedBox(height: 32),

                      // Personal Information Section
                      _sectionLabel(l10n.personalInformation),
                      const SizedBox(height: 16),

                      _textField(
                        label: l10n.fullName,
                        controller: _fullNameController,
                        validator: _validateFullName,
                        enabled: !isUpdating,
                        icon: Icons.person_outline,
                      ),
                      const SizedBox(height: 16),

                      Row(
                        children: [
                          Expanded(
                            child: DropdownButtonFormField<String>(
                              value: _selectedGender,
                              validator: _validateGender,
                              decoration: _inputDecoration(
                                l10n.gender,
                                icon: Icons.wc,
                              ),
                              items: [
                                DropdownMenuItem(
                                  value: "Male",
                                  child: Text(l10n.male),
                                ),
                                DropdownMenuItem(
                                  value: "Female",
                                  child: Text(l10n.female),
                                ),
                              ],
                              onChanged: isUpdating
                                  ? null
                                  : (value) {
                                      setState(() {
                                        _selectedGender = value;
                                      });
                                    },
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: TextFormField(
                              controller: _dobController,
                              readOnly: true,
                              enabled: !isUpdating,
                              validator: _validateDob,
                              onTap: isUpdating ? null : _selectDate,
                              decoration: _inputDecoration(
                                l10n.dateOfBirth,
                                icon: Icons.calendar_today,
                              ).copyWith(
                                suffixIcon: const Icon(
                                  Icons.calendar_today,
                                  size: 18,
                                  color: AppColors.primary,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 32),

                      // Update Button
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: isUpdating ? null : _onSubmit,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.primary,
                            padding: const EdgeInsets.symmetric(vertical: 16),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                            elevation: 0,
                          ),
                          child: isUpdating
                              ? const SizedBox(
                                  height: 22,
                                  width: 22,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2.5,
                                    valueColor: AlwaysStoppedAnimation<Color>(
                                      Colors.white,
                                    ),
                                  ),
                                )
                              : Text(
                                  l10n.updateProfile,
                                  style: TextStyle(
                                    fontSize: 18,
                                    fontWeight: FontWeight.w600,
                                    color: Colors.white,
                                  ),
                                ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  Widget _sectionLabel(String text) {
    return Text(
      text,
      style: TextStyle(
        color: AppColors.textPrimary,
        fontSize: 16,
        fontWeight: FontWeight.w700,
        fontFamily: GoogleFonts.inter().fontFamily,
      ),
    );
  }

  InputDecoration _inputDecoration(String label, {IconData? icon}) {
    return InputDecoration(
      labelText: label,
      prefixIcon:
          icon != null ? Icon(icon, color: AppColors.primary, size: 20) : null,
      floatingLabelBehavior: FloatingLabelBehavior.never,
      labelStyle: TextStyle(
        color: AppColors.textSecondary,
        fontSize: 14,
      ),
      filled: true,
      fillColor: AppColors.surface,
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(
          color: AppColors.border,
          width: 1.4,
        ),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: AppColors.primary,
          width: 1.6,
        ),
      ),
      disabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(
          color: AppColors.lightGrey,
          width: 1.4,
        ),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: AppColors.error,
        ),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(
          color: AppColors.error,
        ),
      ),
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
    );
  }

  Widget _textField({
    required String label,
    required TextEditingController controller,
    String? Function(String?)? validator,
    bool enabled = true,
    IconData? icon,
  }) {
    return TextFormField(
      controller: controller,
      validator: validator,
      enabled: enabled,
      decoration: _inputDecoration(label, icon: icon),
      style: TextStyle(
        color: enabled ? AppColors.textPrimary : AppColors.textSecondary,
      ),
    );
  }

  Widget _avatarFallback() {
    return Icon(
      Icons.person,
      size: 60,
      color: AppColors.lightGrey,
    );
  }
}
