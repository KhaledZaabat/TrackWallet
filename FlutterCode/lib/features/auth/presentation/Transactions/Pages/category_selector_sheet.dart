import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/transaction_type_button.dart';
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
    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          _buildHandle(),
          _buildHeader(),
          _buildSearchBar(),
          _buildGroupFilters(),
          _buildCategoryGrid(),
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
          color: Colors.grey.shade300,
          borderRadius: BorderRadius.circular(3),
        ),
      ),
    );
  }

  Widget _buildHeader() {
    return const Padding(
      padding: EdgeInsets.symmetric(horizontal: 20, vertical: 12),
      child: Text(
        'Select Category',
        style: TextStyle(
          fontSize: 20,
          fontWeight: FontWeight.w700,
          color: Color(0xFF2D3436),
        ),
      ),
    );
  }

  Widget _buildSearchBar() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
      child: TextField(
        controller: _searchController,
        decoration: InputDecoration(
          prefixIcon: const Icon(
            Icons.search_rounded,
            color: Color(0xFF636E72),
          ),
          suffixIcon: _searchController.text.isNotEmpty
              ? IconButton(
                  icon: const Icon(Icons.close_rounded),
                  onPressed: () {
                    _searchController.clear();
                    _filterCategories('');
                  },
                )
              : null,
          hintText: 'Search category',
          hintStyle: const TextStyle(
            color: Color(0xFFB2BEC3),
          ),
          filled: true,
          fillColor: const Color(0xFFF5F8FA),
          contentPadding: const EdgeInsets.symmetric(vertical: 16),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(14),
            borderSide: BorderSide.none,
          ),
        ),
        onChanged: _filterCategories,
      ),
    );
  }

  Widget _buildGroupFilters() {
    return SizedBox(
      height: 48,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
        itemCount: _getGroupNames().length,
        itemBuilder: (context, index) {
          final group = _getGroupNames()[index];
          final isSelected = _selectedGroup == group;
          return Padding(
            padding: const EdgeInsets.only(right: 8),
            child: ChoiceChip(
              label: Text(group),
              selected: isSelected,
              onSelected: (_) => _filterByGroup(group),
              backgroundColor: Colors.white,
              selectedColor: const Color(0xFF6C5CE7),
              labelStyle: TextStyle(
                color: isSelected ? Colors.white : const Color(0xFF2D3436),
                fontWeight: FontWeight.w600,
                fontSize: 13,
              ),
              side: BorderSide(
                color: isSelected
                    ? const Color(0xFF6C5CE7)
                    : const Color(0xFFDFE6E9),
              ),
              padding: const EdgeInsets.symmetric(horizontal: 12),
            ),
          );
        },
      ),
    );
  }

  Widget _buildCategoryGrid() {
    return Flexible(
      child: _filteredCategories.isEmpty
          ? _buildEmptyState()
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

  Widget _buildEmptyState() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.category_outlined,
              size: 64,
              color: const Color(0xFFB2BEC3).withOpacity(0.5),
            ),
            const SizedBox(height: 16),
            const Text(
              'No categories found',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w600,
                color: Color(0xFF636E72),
              ),
            ),
            const SizedBox(height: 8),
            const Text(
              'Try searching with different keywords',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: Color(0xFFB2BEC3),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
