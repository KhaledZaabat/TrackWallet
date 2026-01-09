import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/features/Families/Cubits/create_family_cubit.dart';
import 'package:famxpense/features/Families/Cubits/create_family_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class CreateFamilyPage extends StatelessWidget {
  const CreateFamilyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<CreateFamilyCubit>(),
      child: const _CreateFamilyView(),
    );
  }
}

class _CreateFamilyView extends StatefulWidget {
  const _CreateFamilyView();

  @override
  State<_CreateFamilyView> createState() => _CreateFamilyViewState();
}

class _CreateFamilyViewState extends State<_CreateFamilyView> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _budgetController = TextEditingController();
  final _bioController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _budgetController.dispose();
    _bioController.dispose();
    super.dispose();
  }

  void _handleCreateFamily() {
    if (_formKey.currentState!.validate()) {
      final name = _nameController.text.trim();
      final budget = double.tryParse(_budgetController.text.trim()) ?? 0.0;
      final bio = _bioController.text.trim();

      context.read<CreateFamilyCubit>().createFamily(
            name: name,
            initialBudget: budget,
            familyBio: bio.isNotEmpty ? bio : null,
          );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF5B6B8C)),
          onPressed: () => context.pop(),
        ),
        title: const Text(
          'Create Family',
          style: TextStyle(
            color: Color(0xFF5B6B8C),
            fontWeight: FontWeight.w700,
          ),
        ),
      ),
      body: BlocConsumer<CreateFamilyCubit, CreateFamilyState>(
        listener: (context, state) {
          if (state is CreateFamilyError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
                behavior: SnackBarBehavior.floating,
                margin: const EdgeInsets.all(16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
            );
          }

          if (state is CreateFamilySuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Family "${state.family.name}" created!'),
                backgroundColor: const Color(0xFF27AE60),
                behavior: SnackBarBehavior.floating,
                margin: const EdgeInsets.all(16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
            );

            // Navigate back to select family page
            // The select family page will reload families
            context.pop(true); // Return true to indicate success
          }
        },
        builder: (context, state) {
          final isLoading = state is CreateFamilyLoading;

          return SafeArea(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    // Header
                    Container(
                      padding: const EdgeInsets.all(20),
                      decoration: BoxDecoration(
                        color: const Color(0xFF5B7CB5).withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Column(
                        children: [
                          const Icon(
                            Icons.family_restroom,
                            size: 48,
                            color: Color(0xFF5B7CB5),
                          ),
                          const SizedBox(height: 12),
                          const Text(
                            'Start Your Family Journey',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.w700,
                              color: Color(0xFF5B6B8C),
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            'Create a family to manage expenses together',
                            textAlign: TextAlign.center,
                            style: TextStyle(
                              fontSize: 13,
                              color: const Color(0xFF5B6B8C)
                                  .withValues(alpha: 0.7),
                            ),
                          ),
                        ],
                      ),
                    ),

                    const SizedBox(height: 32),

                    // Family Name
                    Text(
                      'Family Name *',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: const Color(0xFF5B6B8C).withValues(alpha: 0.8),
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _nameController,
                      enabled: !isLoading,
                      textCapitalization: TextCapitalization.words,
                      validator: (value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter a family name';
                        }
                        if (value.trim().length < 2) {
                          return 'Family name must be at least 2 characters';
                        }
                        return null;
                      },
                      decoration: InputDecoration(
                        hintText: 'The Smith Family',
                        hintStyle: TextStyle(
                          color: const Color(0xFF5B6B8C).withValues(alpha: 0.3),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFFE0E5EB),
                            width: 1.5,
                          ),
                        ),
                        enabledBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFFE0E5EB),
                            width: 1.5,
                          ),
                        ),
                        focusedBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFF5B7CB5),
                            width: 1.5,
                          ),
                        ),
                        errorBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Colors.red,
                            width: 1.5,
                          ),
                        ),
                        focusedErrorBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Colors.red,
                            width: 1.5,
                          ),
                        ),
                        contentPadding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 16,
                        ),
                      ),
                    ),

                    const SizedBox(height: 24),

                    // Initial Budget
                    Text(
                      'Initial Budget *',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: const Color(0xFF5B6B8C).withValues(alpha: 0.8),
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _budgetController,
                      enabled: !isLoading,
                      keyboardType: const TextInputType.numberWithOptions(
                        decimal: true,
                      ),
                      inputFormatters: [
                        FilteringTextInputFormatter.allow(
                          RegExp(r'^\d+\.?\d{0,2}'),
                        ),
                      ],
                      validator: (value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter an initial budget';
                        }
                        final budget = double.tryParse(value.trim());
                        if (budget == null) {
                          return 'Please enter a valid number';
                        }
                        if (budget < 0) {
                          return 'Budget cannot be negative';
                        }
                        return null;
                      },
                      decoration: InputDecoration(
                        hintText: '1000.00',
                        hintStyle: TextStyle(
                          color: const Color(0xFF5B6B8C).withValues(alpha: 0.3),
                        ),
                        prefixText: '\$ ',
                        prefixStyle: const TextStyle(
                          color: Color(0xFF5B6B8C),
                          fontWeight: FontWeight.w600,
                        ),
                        filled: true,
                        fillColor: Colors.white,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFFE0E5EB),
                            width: 1.5,
                          ),
                        ),
                        enabledBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFFE0E5EB),
                            width: 1.5,
                          ),
                        ),
                        focusedBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFF5B7CB5),
                            width: 1.5,
                          ),
                        ),
                        errorBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Colors.red,
                            width: 1.5,
                          ),
                        ),
                        focusedErrorBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Colors.red,
                            width: 1.5,
                          ),
                        ),
                        contentPadding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 16,
                        ),
                      ),
                    ),

                    const SizedBox(height: 24),

                    // Family Bio (Optional)
                    Text(
                      'Family Bio (Optional)',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: const Color(0xFF5B6B8C).withValues(alpha: 0.8),
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _bioController,
                      enabled: !isLoading,
                      maxLines: 3,
                      maxLength: 200,
                      textCapitalization: TextCapitalization.sentences,
                      decoration: InputDecoration(
                        hintText: 'A short description of your family...',
                        hintStyle: TextStyle(
                          color: const Color(0xFF5B6B8C).withValues(alpha: 0.3),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFFE0E5EB),
                            width: 1.5,
                          ),
                        ),
                        enabledBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFFE0E5EB),
                            width: 1.5,
                          ),
                        ),
                        focusedBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(6),
                          borderSide: const BorderSide(
                            color: Color(0xFF5B7CB5),
                            width: 1.5,
                          ),
                        ),
                        contentPadding: const EdgeInsets.all(16),
                      ),
                    ),

                    const SizedBox(height: 32),

                    // Create Button
                    SizedBox(
                      height: 52,
                      child: ElevatedButton(
                        onPressed: isLoading ? null : _handleCreateFamily,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xFF5B7CB5),
                          foregroundColor: Colors.white,
                          elevation: 0,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(6),
                          ),
                          disabledBackgroundColor:
                              const Color(0xFF5B7CB5).withValues(alpha: 0.6),
                        ),
                        child: isLoading
                            ? const SizedBox(
                                width: 22,
                                height: 22,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2.5,
                                  valueColor: AlwaysStoppedAnimation<Color>(
                                    Colors.white,
                                  ),
                                ),
                              )
                            : const Text(
                                'Create Family',
                                style: TextStyle(
                                  fontSize: 17,
                                  fontWeight: FontWeight.w600,
                                  letterSpacing: 0.3,
                                ),
                              ),
                      ),
                    ),

                    const SizedBox(height: 16),

                    // Info text
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: const Color(0xFF5B7CB5).withValues(alpha: 0.05),
                        borderRadius: BorderRadius.circular(6),
                      ),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Icon(
                            Icons.info_outline,
                            size: 16,
                            color:
                                const Color(0xFF5B7CB5).withValues(alpha: 0.7),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'You will be assigned as the parent of this family and can invite other members later.',
                              style: TextStyle(
                                fontSize: 12,
                                color: const Color(0xFF5B6B8C)
                                    .withValues(alpha: 0.7),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
