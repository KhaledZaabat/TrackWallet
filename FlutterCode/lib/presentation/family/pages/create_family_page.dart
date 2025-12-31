import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/presentation/family/cubit/family_cubit.dart';
import 'package:famxpense/presentation/family/cubit/family_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class CreateFamilyPage extends StatefulWidget {
  const CreateFamilyPage({super.key});

  @override
  State<CreateFamilyPage> createState() =>
      _CreateFamilyPageState();
}

class _CreateFamilyPageState extends State<CreateFamilyPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _budgetController = TextEditingController();
  bool _submitted = false;

  @override
  void dispose() {
    _nameController.dispose();
    _budgetController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (!_formKey.currentState!.validate()) return;

    final budget =
        double.tryParse(_budgetController.text.trim()) ?? 0;

    setState(() {
      _submitted = true;
    });

    context.read<FamilyCubit>().createFamily(
          name: _nameController.text.trim(),
          currentBudget: budget,
        );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: BlocConsumer<FamilyCubit, FamilyState>(
        listener: (context, state) {
          if (_submitted &&
              !state.isSaving &&
              state.error == null) {
            context.pop();
          }
        },
        builder: (context, state) {
          return CustomScrollView(
            slivers: [
              MyAppBar(
                title: 'Create Family',
                leading: const Icon(Icons.arrow_back),
                leadingOnPressed: () => context.pop(),
              ),
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 22, vertical: 20),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment:
                          CrossAxisAlignment.stretch,
                      children: [
                        TextFormField(
                          controller: _nameController,
                          textInputAction:
                              TextInputAction.next,
                          validator: (value) {
                            if (value == null ||
                                value.trim().isEmpty) {
                              return 'Family name is required';
                            }
                            return null;
                          },
                          decoration: _inputDecoration(
                              'Family name'),
                        ),
                        const SizedBox(height: 14),
                        TextFormField(
                          controller: _budgetController,
                          keyboardType:
                              const TextInputType.numberWithOptions(
                            decimal: true,
                          ),
                          validator: (value) {
                            if (value == null ||
                                value.trim().isEmpty) {
                              return null;
                            }
                            final parsed = double.tryParse(
                                value.trim());
                            if (parsed == null) {
                              return 'Enter a valid number';
                            }
                            return null;
                          },
                          decoration: _inputDecoration(
                                  'Starting budget')
                              .copyWith(
                            prefixText: '\$ ',
                            prefixStyle: const TextStyle(
                              color: AppColors.mainGrayShade,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),
                        if (state.error != null)
                          Padding(
                            padding:
                                const EdgeInsets.only(top: 8),
                            child: Text(
                              state.error!,
                              style: const TextStyle(
                                color: Colors.redAccent,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                        SizedBox(
                          height: 52,
                          child: ElevatedButton(
                            onPressed: state.isSaving
                                ? null
                                : _onSubmit,
                            style: ElevatedButton.styleFrom(
                              backgroundColor:
                                  AppColors.primary,
                              shape: RoundedRectangleBorder(
                                borderRadius:
                                    BorderRadius.circular(12),
                              ),
                            ),
                            child: state.isSaving
                                ? const SizedBox(
                                    height: 22,
                                    width: 22,
                                    child:
                                        CircularProgressIndicator(
                                      strokeWidth: 2.5,
                                      valueColor:
                                          AlwaysStoppedAnimation<
                                              Color>(
                                        Colors.white,
                                      ),
                                    ),
                                  )
                                : const Text(
                                    'Create',
                                    style: TextStyle(
                                      color: Colors.white,
                                      fontWeight:
                                          FontWeight.w700,
                                      fontSize: 17,
                                    ),
                                  ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  InputDecoration _inputDecoration(String label) {
    return InputDecoration(
      labelText: label,
      floatingLabelBehavior: FloatingLabelBehavior.never,
      labelStyle: const TextStyle(
        color: Color(0xFF8B97A8),
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
      contentPadding: const EdgeInsets.symmetric(
        horizontal: 16,
        vertical: 14,
      ),
    );
  }
}
