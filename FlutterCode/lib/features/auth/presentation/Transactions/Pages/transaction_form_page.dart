import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/category_selector_sheet.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/transaction_type_button.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';

class TransactionFormPage extends StatefulWidget {
  final TransactionItem? existingTransaction;

  const TransactionFormPage({
    super.key,
    this.existingTransaction,
  });

  @override
  State<TransactionFormPage> createState() => _TransactionFormPageState();
}

class _TransactionFormPageState extends State<TransactionFormPage> {
  final _formKey = GlobalKey<FormState>();
  final _amountController = TextEditingController();
  final _titleController = TextEditingController();
  final _notesController = TextEditingController();

  TransactionType _selectedType = TransactionType.Expense;
  CategoryData? _selectedCategory;
  DateTime _selectedDate = DateTime.now();

  bool get _isEditing => widget.existingTransaction != null;

  @override
  void initState() {
    super.initState();
    _initializeFormData();
  }

  void _initializeFormData() {
    if (_isEditing) {
      final tx = widget.existingTransaction!;
      _amountController.text = tx.amount.toStringAsFixed(2);
      _titleController.text = tx.title ?? '';
      _notesController.text = tx.notes ?? '';
      _selectedType = tx.type;
      _selectedDate = tx.transactedOn;

      // Load category
      final categoryService = getIt<CategoryService>();
      _selectedCategory =
          categoryService.getCategoryById(tx.category.categoryId);
    }
  }

  @override
  void dispose() {
    _amountController.dispose();
    _titleController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _selectCategory() async {
    final selected = await showModalBottomSheet<CategoryData>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => CategorySelectorSheet(
        selectedCategory: _selectedCategory,
      ),
    );

    if (selected != null) {
      setState(() => _selectedCategory = selected);
    }
  }

  Future<void> _selectDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime(2000),
      lastDate: DateTime(2100),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: Color(0xFF6C5CE7),
              onPrimary: Colors.white,
              surface: Colors.white,
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() => _selectedDate = picked);
    }
  }

  void _handleSubmit() {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedCategory == null) {
      _showSnackBar('Please select a category', isError: true);
      return;
    }

    final amount = double.tryParse(_amountController.text) ?? 0;
    if (amount <= 0) {
      _showSnackBar('Amount must be greater than zero', isError: true);
      return;
    }

    final cubit = context.read<TransactionCubit>();

    if (_isEditing) {
      final request = UpdateTransactionRequest(
        type: _selectedType,
        amount: amount,
        transactedOn: _selectedDate,
        title: _titleController.text.trim().isEmpty
            ? null
            : _titleController.text.trim(),
        notes: _notesController.text.trim().isEmpty
            ? null
            : _notesController.text.trim(),
        categoryId: _selectedCategory!.categoryId,
      );
      cubit.updateTransaction(
          widget.existingTransaction!.transactionId, request);
    } else {
      final request = CreateTransactionRequest(
        type: _selectedType,
        categoryId: _selectedCategory!.categoryId,
        amount: amount,
        transactedOn: _selectedDate,
        title: _titleController.text.trim().isEmpty
            ? null
            : _titleController.text.trim(),
        notes: _notesController.text.trim().isEmpty
            ? null
            : _notesController.text.trim(),
      );
      cubit.createTransaction(request);
    }
  }

  Future<void> _handleDelete() async {
    final confirmed = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (context) => const DeleteConfirmationDialog(),
    );

    if (confirmed == true && mounted) {
      context.read<TransactionCubit>().deleteTransaction(
            widget.existingTransaction!.transactionId,
          );
    }
  }

  void _showSnackBar(String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? Colors.red.shade400 : Colors.green.shade400,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        margin: const EdgeInsets.all(16),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return BlocListener<TransactionCubit, TransactionState>(
      listener: (context, state) {
        if (state is TransactionOperationSuccess) {
          _showSnackBar(
            _isEditing
                ? 'Transaction updated successfully'
                : 'Transaction created successfully',
          );
          Navigator.of(context).pop(true);
        } else if (state is TransactionOperationError) {
          _showSnackBar(state.message, isError: true);
        }
      },
      child: Scaffold(
        backgroundColor: const Color(0xFFF5F8FA),
        appBar: _buildAppBar(),
        body: _buildBody(),
        bottomNavigationBar: _buildBottomBar(),
      ),
    );
  }

  PreferredSizeWidget _buildAppBar() {
    return AppBar(
      backgroundColor: Colors.white,
      elevation: 0,
      surfaceTintColor: Colors.transparent,
      leading: IconButton(
        icon: const Icon(Icons.arrow_back_ios_new_rounded, size: 20),
        onPressed: () => Navigator.of(context).pop(),
      ),
      centerTitle: true,
      title: Text(
        _isEditing ? 'Edit Transaction' : 'New Transaction',
        style: const TextStyle(
          fontWeight: FontWeight.w700,
          fontSize: 18,
          color: Color(0xFF2D3436),
        ),
      ),
      actions: _isEditing
          ? [
              IconButton(
                icon: const Icon(Icons.delete_outline_rounded),
                color: Colors.red.shade400,
                onPressed: _handleDelete,
              ),
            ]
          : null,
    );
  }

  Widget _buildBody() {
    return BlocBuilder<TransactionCubit, TransactionState>(
      builder: (context, state) {
        final isLoading = state is TransactionOperationInProgress;

        return SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildTypeSelector(),
                const SizedBox(height: 24),
                _buildAmountField(),
                const SizedBox(height: 20),
                _buildTitleField(),
                const SizedBox(height: 20),
                _buildCategorySelector(),
                const SizedBox(height: 20),
                _buildDateSelector(),
                const SizedBox(height: 20),
                _buildNotesField(),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildTypeSelector() {
    return Row(
      children: [
        Expanded(
          child: TransactionTypeButton(
            label: 'Expense',
            icon: Icons.arrow_downward_rounded,
            color: const Color(0xFFE74C3C),
            isSelected: _selectedType == TransactionType.Expense,
            onTap: () =>
                setState(() => _selectedType = TransactionType.Expense),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: TransactionTypeButton(
            label: 'Income',
            icon: Icons.arrow_upward_rounded,
            color: const Color(0xFF27AE60),
            isSelected: _selectedType == TransactionType.Income,
            onTap: () => setState(() => _selectedType = TransactionType.Income),
          ),
        ),
      ],
    );
  }

  Widget _buildAmountField() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Amount',
          style: TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 14,
            color: Color(0xFF2D3436),
          ),
        ),
        const SizedBox(height: 8),
        TextFormField(
          controller: _amountController,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(
            hintText: '0.00',
            prefixIcon: const Icon(Icons.attach_money_rounded),
            filled: true,
            fillColor: Colors.white,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDFE6E9)),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDFE6E9)),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFF6C5CE7), width: 2),
            ),
          ),
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'Please enter an amount';
            }
            final amount = double.tryParse(value);
            if (amount == null || amount <= 0) {
              return 'Please enter a valid amount';
            }
            return null;
          },
        ),
      ],
    );
  }

  Widget _buildTitleField() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Title (Optional)',
          style: TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 14,
            color: Color(0xFF2D3436),
          ),
        ),
        const SizedBox(height: 8),
        TextFormField(
          controller: _titleController,
          decoration: InputDecoration(
            hintText: 'What is this transaction?',
            prefixIcon: const Icon(Icons.title_rounded),
            filled: true,
            fillColor: Colors.white,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDFE6E9)),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDFE6E9)),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFF6C5CE7), width: 2),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildCategorySelector() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Category',
          style: TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 14,
            color: Color(0xFF2D3436),
          ),
        ),
        const SizedBox(height: 8),
        InkWell(
          onTap: _selectCategory,
          borderRadius: BorderRadius.circular(12),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: const Color(0xFFDFE6E9)),
            ),
            child: Row(
              children: [
                Icon(
                  _selectedCategory?.icon ?? Icons.category_outlined,
                  color: const Color(0xFF6C5CE7),
                  size: 24,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    _selectedCategory?.displayName ?? 'Select Category',
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 15,
                      color: _selectedCategory != null
                          ? const Color(0xFF2D3436)
                          : const Color(0xFFB2BEC3),
                    ),
                  ),
                ),
                const Icon(
                  Icons.arrow_forward_ios_rounded,
                  size: 16,
                  color: Color(0xFFB2BEC3),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildDateSelector() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Date',
          style: TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 14,
            color: Color(0xFF2D3436),
          ),
        ),
        const SizedBox(height: 8),
        InkWell(
          onTap: _selectDate,
          borderRadius: BorderRadius.circular(12),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: const Color(0xFFDFE6E9)),
            ),
            child: Row(
              children: [
                const Icon(
                  Icons.calendar_today_rounded,
                  color: Color(0xFF6C5CE7),
                  size: 22,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    DateFormat('EEE, MMM d, yyyy').format(_selectedDate),
                    style: const TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 15,
                      color: Color(0xFF2D3436),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildNotesField() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Notes (Optional)',
          style: TextStyle(
            fontWeight: FontWeight.w700,
            fontSize: 14,
            color: Color(0xFF2D3436),
          ),
        ),
        const SizedBox(height: 8),
        TextFormField(
          controller: _notesController,
          maxLines: 4,
          decoration: InputDecoration(
            hintText: 'Add any additional details...',
            filled: true,
            fillColor: Colors.white,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDFE6E9)),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDFE6E9)),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFF6C5CE7), width: 2),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildBottomBar() {
    return BlocBuilder<TransactionCubit, TransactionState>(
      builder: (context, state) {
        final isLoading = state is TransactionOperationInProgress;

        return SafeArea(
          minimum: const EdgeInsets.all(16),
          child: SizedBox(
            width: double.infinity,
            height: 56,
            child: ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF6C5CE7),
                foregroundColor: Colors.white,
                elevation: 0,
                shadowColor: const Color(0xFF6C5CE7).withOpacity(0.4),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
              ),
              onPressed: isLoading ? null : _handleSubmit,
              child: isLoading
                  ? const SizedBox(
                      width: 24,
                      height: 24,
                      child: CircularProgressIndicator(
                        strokeWidth: 2.5,
                        valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                      ),
                    )
                  : Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          _isEditing
                              ? Icons.check_circle_outline_rounded
                              : Icons.add_circle_outline_rounded,
                          size: 22,
                        ),
                        const SizedBox(width: 8),
                        Text(
                          _isEditing ? 'Update Transaction' : 'Add Transaction',
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w700,
                            letterSpacing: 0.3,
                          ),
                        ),
                      ],
                    ),
            ),
          ),
        );
      },
    );
  }
}
