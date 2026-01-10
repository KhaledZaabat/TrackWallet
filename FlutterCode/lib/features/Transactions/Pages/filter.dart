import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/data/repos/transaction_repository.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class TransactionFilterSheet extends StatefulWidget {
  final TransactionFilters currentFilters;

  const TransactionFilterSheet({
    super.key,
    required this.currentFilters,
  });

  @override
  State<TransactionFilterSheet> createState() => _TransactionFilterSheetState();
}

class _TransactionFilterSheetState extends State<TransactionFilterSheet>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;

  TransactionType? _selectedType;
  String? _selectedCategoryGroup;
  double? _minAmount;
  double? _maxAmount;
  String? _selectedCreatorId;

  final _minAmountController = TextEditingController();
  final _maxAmountController = TextEditingController();

  List<FamilyUser> _familyUsers = [];
  bool _isLoadingUsers = false;

  late final CategoryService _categoryService;

  @override
  void initState() {
    super.initState();
    _categoryService = getIt<CategoryService>();
    _initializeFilters();
    _loadFamilyUsers();

    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 300),
    );

    _fadeAnimation = CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeInOut,
    );

    _animationController.forward();
  }

  void _initializeFilters() {
    _selectedType = widget.currentFilters.transactionType;
    _selectedCategoryGroup = widget.currentFilters.categoryType;
    _minAmount = widget.currentFilters.minAmount;
    _maxAmount = widget.currentFilters.maxAmount;
    _selectedCreatorId = widget.currentFilters.creatorId;

    if (_minAmount != null) {
      _minAmountController.text = _minAmount!.toStringAsFixed(2);
    }
    if (_maxAmount != null) {
      _maxAmountController.text = _maxAmount!.toStringAsFixed(2);
    }
  }

  Future<void> _loadFamilyUsers() async {
    setState(() => _isLoadingUsers = true);

    try {
      final repository = getIt<TransactionRepository>();
      final result = await repository.getFamilyUsers();

      if (result.isSuccess && result.users != null) {
        setState(() {
          _familyUsers = result.users!;
          _isLoadingUsers = false;
        });
      } else {
        setState(() => _isLoadingUsers = false);
      }
    } catch (e) {
      setState(() => _isLoadingUsers = false);
    }
  }

  @override
  void dispose() {
    _animationController.dispose();
    _minAmountController.dispose();
    _maxAmountController.dispose();
    super.dispose();
  }

  void _applyFilters() {
    final filters = TransactionFilters(
      transactionType: _selectedType,
      categoryType: _selectedCategoryGroup,
      minAmount: _minAmount,
      maxAmount: _maxAmount,
      creatorId: _selectedCreatorId,
    );

    Navigator.of(context).pop(filters);
  }

  void _clearAllFilters() {
    setState(() {
      _selectedType = null;
      _selectedCategoryGroup = null;
      _minAmount = null;
      _maxAmount = null;
      _selectedCreatorId = null;
      _minAmountController.clear();
      _maxAmountController.clear();
    });
  }

  int get _activeFilterCount {
    int count = 0;
    if (_selectedType != null) count++;
    if (_selectedCategoryGroup != null) count++;
    if (_minAmount != null || _maxAmount != null) count++;
    if (_selectedCreatorId != null) count++;
    return count;
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return FadeTransition(
      opacity: _fadeAnimation,
      child: Container(
        decoration: const BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _buildHandle(),
            _buildHeader(l10n),
            Flexible(
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(20, 0, 20, 20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _buildTransactionTypeFilter(l10n),
                    const SizedBox(height: 24),
                    _buildCategoryFilter(l10n),
                    const SizedBox(height: 24),
                    _buildAmountRangeFilter(l10n),
                    const SizedBox(height: 24),
                    _buildCreatorFilter(l10n),
                    const SizedBox(height: 32),
                  ],
                ),
              ),
            ),
            _buildBottomActions(l10n),
          ],
        ),
      ),
    );
  }

  Widget _buildHandle() {
    return Padding(
      padding: const EdgeInsets.only(top: 12, bottom: 8),
      child: Container(
        width: 48,
        height: 5,
        decoration: BoxDecoration(
          color: AppColors.border,
          borderRadius: BorderRadius.circular(3),
        ),
      ),
    );
  }

  Widget _buildHeader(AppLocalizations l10n) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
      child: Row(
        children: [
          Text(
            l10n.filterTransactions,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w700,
              color: AppColors.textPrimary,
            ),
          ),
          const Spacer(),
          if (_activeFilterCount > 0)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: AppColors.primary.withOpacity(0.15),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                l10n.filtersActive(_activeFilterCount),
                style: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w700,
                  color: AppColors.primary,
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildTransactionTypeFilter(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          l10n.transactionType,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w700,
            color: AppColors.textPrimary,
          ),
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: _FilterChip(
                label: l10n.all,
                icon: Icons.all_inclusive_rounded,
                isSelected: _selectedType == null,
                onTap: () => setState(() => _selectedType = null),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _FilterChip(
                label: l10n.income,
                icon: Icons.arrow_upward_rounded,
                color: AppColors.success,
                isSelected: _selectedType == TransactionType.Income,
                onTap: () =>
                    setState(() => _selectedType = TransactionType.Income),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _FilterChip(
                label: l10n.expense,
                icon: Icons.arrow_downward_rounded,
                color: AppColors.error,
                isSelected: _selectedType == TransactionType.Expense,
                onTap: () =>
                    setState(() => _selectedType = TransactionType.Expense),
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildCategoryFilter(AppLocalizations l10n) {
    final categoryGroups = _getCategoryGroups();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          l10n.categoryGroup,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w700,
            color: AppColors.textPrimary,
          ),
        ),
        const SizedBox(height: 12),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            _FilterChip(
              label: l10n.allCategories,
              icon: Icons.category_outlined,
              isSelected: _selectedCategoryGroup == null,
              onTap: () => setState(() => _selectedCategoryGroup = null),
            ),
            ...categoryGroups.map((group) {
              return _FilterChip(
                label: group.name,
                icon: group.icon,
                isSelected: _selectedCategoryGroup == group.name,
                onTap: () =>
                    setState(() => _selectedCategoryGroup = group.name),
              );
            }),
          ],
        ),
      ],
    );
  }

  List<_CategoryGroup> _getCategoryGroups() {
    return [
      _CategoryGroup('Food & Drinks', Icons.restaurant_rounded),
      _CategoryGroup('Transportation', Icons.directions_car_rounded),
      _CategoryGroup('Bills & Utilities', Icons.receipt_long_rounded),
      _CategoryGroup('Housing', Icons.home_rounded),
      _CategoryGroup('Shopping', Icons.shopping_bag_rounded),
      _CategoryGroup('Entertainment', Icons.movie_rounded),
      _CategoryGroup('Health', Icons.favorite_rounded),
      _CategoryGroup('Education & Work', Icons.school_rounded),
      _CategoryGroup('Finance', Icons.account_balance_rounded),
      _CategoryGroup('Travel', Icons.flight_rounded),
      _CategoryGroup('Family & Pets', Icons.pets_rounded),
      _CategoryGroup('Gifts & Charity', Icons.card_giftcard_rounded),
      _CategoryGroup('Other', Icons.more_horiz_rounded),
    ];
  }

  Widget _buildAmountRangeFilter(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          l10n.amountRange,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w700,
            color: AppColors.textPrimary,
          ),
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: _buildAmountField(
                controller: _minAmountController,
                label: l10n.min,
                hint: '0.00',
                onChanged: (value) {
                  setState(() {
                    _minAmount = double.tryParse(value);
                  });
                },
              ),
            ),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 12),
              child: Icon(
                Icons.arrow_forward_rounded,
                color: AppColors.textSecondary.withOpacity(0.5),
                size: 20,
              ),
            ),
            Expanded(
              child: _buildAmountField(
                controller: _maxAmountController,
                label: l10n.max,
                hint: '0.00',
                onChanged: (value) {
                  setState(() {
                    _maxAmount = double.tryParse(value);
                  });
                },
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildAmountField({
    required TextEditingController controller,
    required String label,
    required String hint,
    required Function(String) onChanged,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: TextStyle(
            fontSize: 13,
            fontWeight: FontWeight.w600,
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: 6),
        TextField(
          controller: controller,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          decoration: InputDecoration(
            hintText: hint,
            prefixIcon: Icon(Icons.attach_money_rounded, size: 20, color: AppColors.textSecondary),
            filled: true,
            fillColor: AppColors.background,
            contentPadding: const EdgeInsets.symmetric(vertical: 14),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide.none,
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: AppColors.border),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: AppColors.primary, width: 2),
            ),
          ),
          inputFormatters: [
            FilteringTextInputFormatter.allow(RegExp(r'^\d*\.?\d{0,2}')),
          ],
          onChanged: onChanged,
        ),
      ],
    );
  }

  Widget _buildCreatorFilter(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          l10n.createdBy,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w700,
            color: AppColors.textPrimary,
          ),
        ),
        const SizedBox(height: 12),
        if (_isLoadingUsers)
          Center(
            child: Padding(
              padding: EdgeInsets.all(16),
              child: CircularProgressIndicator(color: AppColors.primary),
            ),
          )
        else if (_familyUsers.isEmpty)
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.background,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.border),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.info_outline_rounded,
                  color: AppColors.textSecondary.withOpacity(0.5),
                  size: 20,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    l10n.noFamilyMembersFound,
                    style: TextStyle(
                      fontSize: 13,
                      color: AppColors.textSecondary,
                    ),
                  ),
                ),
              ],
            ),
          )
        else
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              _FilterChip(
                label: l10n.allMembers,
                icon: Icons.people_outline_rounded,
                isSelected: _selectedCreatorId == null,
                onTap: () => setState(() => _selectedCreatorId = null),
              ),
              ..._familyUsers.map((user) {
                return _FilterChip(
                  label: user.displayName,
                  icon: Icons.person_outline_rounded,
                  isSelected: _selectedCreatorId == user.userId,
                  onTap: () => setState(() => _selectedCreatorId = user.userId),
                );
              }),
            ],
          ),
      ],
    );
  }

  Widget _buildBottomActions(AppLocalizations l10n) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppColors.surface,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: SafeArea(
        top: false,
        child: Row(
          children: [
            if (_activeFilterCount > 0)
              Expanded(
                child: OutlinedButton(
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    side: const BorderSide(
                      color: AppColors.border,
                      width: 1.5,
                    ),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                  ),
                  onPressed: _clearAllFilters,
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(
                        Icons.clear_all_rounded,
                        size: 20,
                        color: AppColors.textPrimary,
                      ),
                      const SizedBox(width: 8),
                      Text(
                        l10n.clearAll,
                        style: const TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.w600,
                          color: AppColors.textPrimary,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            if (_activeFilterCount > 0) const SizedBox(width: 12),
            Expanded(
              flex: _activeFilterCount > 0 ? 2 : 1,
              child: ElevatedButton(
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  elevation: 0,
                  shadowColor: AppColors.primary.withOpacity(0.4),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
                onPressed: _applyFilters,
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.check_circle_outline_rounded, size: 20),
                    const SizedBox(width: 8),
                    Text(
                      _activeFilterCount > 0
                          ? l10n.applyFiltersCount(_activeFilterCount)
                          : l10n.showAll,
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _FilterChip extends StatelessWidget {
  final String label;
  final IconData icon;
  final Color? color;
  final bool isSelected;
  final VoidCallback onTap;

  const _FilterChip({
    required this.label,
    required this.icon,
    this.color,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final chipColor = color ?? AppColors.primary;

    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        curve: Curves.easeInOut,
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
        decoration: BoxDecoration(
          color: isSelected ? chipColor : AppColors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? chipColor : AppColors.border,
            width: isSelected ? 0 : 1.5,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: chipColor.withOpacity(0.3),
                    blurRadius: 8,
                    offset: const Offset(0, 4),
                  ),
                ]
              : null,
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              icon,
              color: isSelected ? Colors.white : AppColors.textPrimary,
              size: 18,
            ),
            const SizedBox(width: 6),
            Text(
              label,
              style: TextStyle(
                color: isSelected ? Colors.white : AppColors.textPrimary,
                fontSize: 13,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _CategoryGroup {
  final String name;
  final IconData icon;

  _CategoryGroup(this.name, this.icon);
}
