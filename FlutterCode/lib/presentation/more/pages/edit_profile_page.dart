import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/domain/entities/user.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/validation_patterns.dart';
import 'package:famxpense/presentation/more/cubit/user_cubit.dart';
import 'package:famxpense/presentation/more/cubit/user_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

class EditProfilePage extends StatefulWidget {
  const EditProfilePage({super.key});

  @override
  State<EditProfilePage> createState() => _EditProfilePageState();
}

class _EditProfilePageState extends State<EditProfilePage> {
  final _formKey = GlobalKey<FormState>();
  final _fullNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _dobController = TextEditingController();

  DateTime? _selectedDob;
  String? _selectedGender;
  String? _loadedUserId;

  @override
  void initState() {
    super.initState();
    final cubit = context.read<UserCubit>();
    final user = cubit.state.user;

    if (user != null) {
      _hydrateForm(user);
    } else {
      cubit.loadCurrentUser();
    }
  }

  @override
  void dispose() {
    _fullNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _dobController.dispose();
    super.dispose();
  }

  void _hydrateForm(User user) {
    if (!mounted) return;

    setState(() {
      _loadedUserId = user.id;
      _fullNameController.text = user.fullName;
      _emailController.text = user.email;
      _selectedDob = user.dateOfBirth;
      _selectedGender = user.isMale ? 'Male' : 'Female';
      _dobController.text = DateFormat('yyyy-MM-dd').format(user.dateOfBirth);
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
              onSurface: AppColors.mainBlackShade,
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

  void _onSubmit() {
    FocusScope.of(context).unfocus();

    if (!_formKey.currentState!.validate()) return;

    final cubit = context.read<UserCubit>();
    final user = cubit.state.user;

    if (user == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cannot update profile before loading user'),
        ),
      );
      return;
    }

    cubit.updateProfile(
      fullName: _fullNameController.text.trim(),
      isMale: _selectedGender == 'Male',
      dateOfBirth: _selectedDob ?? user.dateOfBirth,
      email: _emailController.text.trim(),
      password: _passwordController.text.trim().isEmpty
          ? null
          : _passwordController.text.trim(),
      profilePictureUrl: user.profilePictureUrl,
    );
  }

  void _onStateChanged(BuildContext context, UserState state) {
    final user = state.user;

    if (user != null && user.id != _loadedUserId) {
      _hydrateForm(user);
    }

    if (state.successMessage != null) {
      _passwordController.clear();
      if (Navigator.of(context).canPop()) {
        context.pop();
      }
    }

    if (state.error != null &&
        !state.isLoading &&
        !state.isSaving &&
        state.user != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(state.error!)),
      );
    }
  }

  String? _validateFullName(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Full name is required';
    }
    if (value.trim().length < 3) {
      return 'Full name must be at least 3 characters';
    }
    return null;
  }

  String? _validateEmail(String? value) {
    return ValidationPatterns.validateEmail(value);
  }

  String? _validateDob(String? _) {
    if (_selectedDob == null) {
      return 'Please select your birth date';
    }
    return null;
  }

  String? _validateGender(String? value) {
    if (value == null || value.isEmpty) {
      return 'Please select gender';
    }
    return null;
  }

  String? _validatePassword(String? value) {
    if (value == null || value.isEmpty) {
      return null;
    }
    if (!ValidationPatterns.isStrongPassword(value)) {
      return 'Password must include upper, lower, digit & special char';
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: SafeArea(
        child: BlocConsumer<UserCubit, UserState>(
          listener: _onStateChanged,
          builder: (context, state) {
            if (state.isLoading && state.user == null) {
              return const Center(child: CircularProgressIndicator());
            }

            if (state.error != null && state.user == null) {
              return Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(state.error!),
                    const SizedBox(height: 10),
                    TextButton(
                      onPressed: () =>
                          context.read<UserCubit>().loadCurrentUser(),
                      child: const Text('Retry'),
                    ),
                  ],
                ),
              );
            }

            final user = state.user;
            final hasProfilePicture =
                (user?.profilePictureUrl.isNotEmpty ?? false);

            return SingleChildScrollView(
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 22, vertical: 32),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      GestureDetector(
                        onTap: () => context.pop(),
                        child: Icon(
                          Icons.arrow_back,
                          size: 24,
                          color: Colors.grey.shade700,
                        ),
                      ),
                      const SizedBox(height: 20),
                      Text(
                        "Edit Profile",
                        style: TextStyle(
                          fontSize: 32,
                          fontWeight: FontWeight.w700,
                          fontFamily: GoogleFonts.inter().fontFamily,
                        ),
                      ),
                      const SizedBox(height: 24),
                      Center(
                        child: CircleAvatar(
                          radius: 40,
                          backgroundColor: AppColors.stroke,
                          child: hasProfilePicture
                              ? ClipOval(
                                  child: Image.network(
                                    user!.profilePictureUrl,
                                    width: 80,
                                    height: 80,
                                    fit: BoxFit.cover,
                                    errorBuilder: (_, __, ___) =>
                                        _avatarFallback(),
                                  ),
                                )
                              : _avatarFallback(),
                        ),
                      ),
                      const SizedBox(height: 28),
                      _sectionLabel("Personal"),
                      const SizedBox(height: 12),
                      _textField(
                        label: "Full Name",
                        controller: _fullNameController,
                        validator: _validateFullName,
                        textInputAction: TextInputAction.next,
                      ),
                      const SizedBox(height: 12),
                      Row(
                        children: [
                          Expanded(
                            child: DropdownButtonFormField<String>(
                              initialValue: _selectedGender,
                              validator: _validateGender,
                              decoration: _inputDecoration("Gender"),
                              items: const [
                                DropdownMenuItem(
                                  value: "Male",
                                  child: Text("Male"),
                                ),
                                DropdownMenuItem(
                                  value: "Female",
                                  child: Text("Female"),
                                ),
                              ],
                              onChanged: (value) {
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
                              validator: _validateDob,
                              onTap: _selectDate,
                              decoration: _inputDecoration(
                                "Date of Birth",
                              ).copyWith(
                                suffixIcon: const Icon(
                                  Icons.calendar_today,
                                  size: 18,
                                  color: Colors.grey,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 26),
                      _sectionLabel("Credentials"),
                      const SizedBox(height: 12),
                      _textField(
                        label: "Email",
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        validator: _validateEmail,
                        textInputAction: TextInputAction.next,
                      ),
                      const SizedBox(height: 12),
                      _textField(
                        label: "New Password (optional)",
                        controller: _passwordController,
                        obscure: true,
                        validator: _validatePassword,
                        textInputAction: TextInputAction.done,
                      ),
                      const SizedBox(height: 30),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: state.isSaving ? null : _onSubmit,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.primary,
                            padding: const EdgeInsets.symmetric(
                              vertical: 14,
                            ),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                          ),
                          child: state.isSaving
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
                              : const Text(
                                  "Confirm",
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
        color: AppColors.mainBlackShade,
        fontSize: 15,
        fontWeight: FontWeight.w700,
        fontFamily: GoogleFonts.inter().fontFamily,
      ),
    );
  }

  InputDecoration _inputDecoration(String label) {
    return InputDecoration(
      labelText: label,
      floatingLabelBehavior: FloatingLabelBehavior.never,
      labelStyle: TextStyle(
        color: const Color(0xFF8B97A8),
        fontSize: 14,
      ),
      filled: true,
      fillColor: Colors.white,
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: BorderSide(
          color: AppColors.stroke,
          width: 1.4,
        ),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: const BorderSide(
          color: AppColors.primary,
          width: 1.6,
        ),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: const BorderSide(
          color: Colors.redAccent,
        ),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: const BorderSide(
          color: Colors.redAccent,
        ),
      ),
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
    );
  }

  Widget _textField({
    required String label,
    required TextEditingController controller,
    TextInputType? keyboardType,
    bool obscure = false,
    String? Function(String?)? validator,
    TextInputAction? textInputAction,
  }) {
    return TextFormField(
      controller: controller,
      validator: validator,
      obscureText: obscure,
      keyboardType: keyboardType,
      textInputAction: textInputAction,
      decoration: _inputDecoration(label),
    );
  }

  Widget _avatarFallback() {
    return Icon(
      Icons.person,
      size: 40,
      color: Colors.grey.shade500,
    );
  }
}
