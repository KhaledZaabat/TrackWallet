import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/features/Transactions/Pages/transaction_type_button.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';

class CategorySelectorSheet extends StatefulWidget {
  final CategoryData? selectedCategory;

  const CategorySelectorSheet({
    super.key,
    this.selectedCategory,
  });

  @override
  State<CategorySelectorSheet> createState() => _CategorySelectorSheetState();
}

class _CategorySelectorSheetState extends State<CategorySelectorSheet> {
  final _searchController = TextEditingController();
  late final CategoryService _categoryService;
  List<CategoryData> _filteredCategories = [];
  String _selectedGroup = 'All';

  @override
  void initState() {
    super.initState();
    _categoryService = getIt<CategoryService>();
    _filteredCategories = _categoryService.getAllCategories();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _filterCategories(String query) {
    setState(() {
      if (query.isEmpty && _selectedGroup == 'All') {
        _filteredCategories = _categoryService.getAllCategories();
      } else if (query.isEmpty) {
        _filteredCategories = _categoryService
            .getAllCategories()
            .where((cat) =>
                CategoryIconHelper.getGroupName(cat.categoryType) ==
                _selectedGroup)
            .toList();
      } else {
        _filteredCategories = _categoryService.searchCategories(query);
        if (_selectedGroup != 'All') {
          _filteredCategories = _filteredCategories
              .where((cat) =>
                  CategoryIconHelper.getGroupName(cat.categoryType) ==
                  _selectedGroup)
              .toList();
        }
      }
    });
  }

  void _filterByGroup(String group) {
    setState(() {
      _selectedGroup = group;
      _searchController.clear();
      _filterCategories('');
    });
  }

  List<String> _getGroupNames() {
    return [
      'All',
      'Food & Drinks',
      'Transportation',
      'Bills & Utilities',
      'Housing',
      'Shopping',
      'Entertainment',
      'Health',
      'Education & Work',
      'Finance',
      'Travel',
      'Family & Pets',
      'Gifts & Charity',
      'Other',
    ];
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return Container(
      decoration: const BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          _buildHandle(),
          _buildHeader(l10n),
          _buildSearchBar(l10n),
          _buildGroupFilters(l10n),
          _buildCategoryGrid(l10n),
        ],
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
      child: Text(
        l10n.selectCategory,
        style: const TextStyle(
          fontSize: 20,
          fontWeight: FontWeight.w700,
          color: AppColors.textPrimary,
        ),
      ),
    );
  }

  Widget _buildSearchBar(AppLocalizations l10n) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
      child: TextField(
        controller: _searchController,
        decoration: InputDecoration(
          prefixIcon: Icon(
            Icons.search_rounded,
            color: AppColors.textSecondary.withOpacity(0.7),
          ),
          suffixIcon: _searchController.text.isNotEmpty
              ? IconButton(
                  icon: Icon(Icons.close_rounded, color: AppColors.textSecondary),
                  onPressed: () {
                    _searchController.clear();
                    _filterCategories('');
                  },
                )
              : null,
          hintText: l10n.searchCategory,
          hintStyle: TextStyle(
            color: AppColors.textSecondary.withOpacity(0.5),
          ),
          filled: true,
          fillColor: AppColors.background,
          contentPadding: const EdgeInsets.symmetric(vertical: 16),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(14),
            borderSide: BorderSide.none,
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(14),
            borderSide: BorderSide(color: AppColors.primary, width: 1.5),
          ),
        ),
        onChanged: _filterCategories,
      ),
    );
  }

  Widget _buildGroupFilters(AppLocalizations l10n) {
    return SizedBox(
      height: 48,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
        itemCount: _getGroupNames().length,
        itemBuilder: (context, index) {
          final group = _getGroupNames()[index];
          final isSelected = _selectedGroup == group;
          final displayName = group == 'All' ? l10n.all : group;
          return Padding(
            padding: const EdgeInsets.only(right: 8),
            child: ChoiceChip(
              label: Text(displayName),
              selected: isSelected,
              onSelected: (_) => _filterByGroup(group),
              backgroundColor: AppColors.white,
              selectedColor: AppColors.primary,
              labelStyle: TextStyle(
                color: isSelected ? Colors.white : AppColors.textPrimary,
                fontWeight: FontWeight.w600,
                fontSize: 13,
              ),
              side: BorderSide(
                color: isSelected
                    ? AppColors.primary
                    : AppColors.border,
              ),
              padding: const EdgeInsets.symmetric(horizontal: 12),
            ),
          );
        },
      ),
    );
  }

  Widget _buildCategoryGrid(AppLocalizations l10n) {
    return Flexible(
      child: _filteredCategories.isEmpty
          ? _buildEmptyState(l10n)
          : Padding(
              padding: const EdgeInsets.all(20),
              child: GridView.builder(
                shrinkWrap: true,
                physics: const AlwaysScrollableScrollPhysics(),
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  crossAxisSpacing: 12,
                  mainAxisSpacing: 12,
                  childAspectRatio: 3,
                ),
                itemCount: _filteredCategories.length,
                itemBuilder: (context, index) {
                  final category = _filteredCategories[index];
                  return CategoryChip(
                    categoryId: category.categoryId,
                    categoryName: category.displayName,
                    icon: category.icon,
                    isSelected: widget.selectedCategory?.categoryId ==
                        category.categoryId,
                    onTap: () => Navigator.of(context).pop(category),
                  );
                },
              ),
            ),
    );
  }

  Widget _buildEmptyState(AppLocalizations l10n) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.category_outlined,
              size: 64,
              color: AppColors.textSecondary.withOpacity(0.3),
            ),
            const SizedBox(height: 16),
            Text(
              l10n.noCategoriesFound,
              style: const TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w600,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              l10n.tryDifferentKeywords,
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary.withOpacity(0.6),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
