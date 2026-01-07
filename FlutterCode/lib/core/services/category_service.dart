import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/domain/entities/category.dart';

class CategoryService {
  final ApiClient _apiClient;

  // In-memory cache
  Map<String, CategoryData>? _categoriesById;
  List<CategoryData>? _allCategories;
  bool _isInitialized = false;

  CategoryService(this._apiClient);

  /// Initialize categories on app start
  Future<void> initialize() async {
    if (_isInitialized) {
      AppLogger.info('CategoryService', 'Already initialized, skipping...');
      return;
    }

    try {
      AppLogger.info('CategoryService', 'Fetching categories from API...');

      final response = await _apiClient.dio.get('/api/categories');

      if (response.statusCode == 200) {
        final List<dynamic> data = response.data;

        _allCategories =
            data.map((json) => CategoryData.fromJson(json)).toList();
        _categoriesById = {
          for (var category in _allCategories!) category.categoryId: category
        };

        _isInitialized = true;
        AppLogger.info('CategoryService',
            'Successfully loaded ${_allCategories!.length} categories');
      } else {
        throw Exception('Failed to load categories: ${response.statusCode}');
      }
    } catch (e, stackTrace) {
      AppLogger.error(
        'CategoryService',
        'Failed to initialize categories',
        error: e,
        stackTrace: stackTrace,
      );
      // Don't throw - allow app to continue with empty categories
      _allCategories = [];
      _categoriesById = {};
    }
  }

  /// Get category by ID
  CategoryData? getCategoryById(String categoryId) {
    if (!_isInitialized) {
      AppLogger.error(
          'CategoryService', 'Attempted to get category before initialization');
      return null;
    }
    return _categoriesById?[categoryId];
  }

  /// Get all categories
  List<CategoryData> getAllCategories() {
    if (!_isInitialized) {
      AppLogger.error('CategoryService',
          'Attempted to get categories before initialization');
      return [];
    }
    return _allCategories ?? [];
  }

  /// Get category type by ID (for icon mapping)
  CategoryType? getCategoryTypeById(String categoryId) {
    final category = getCategoryById(categoryId);
    return category?.categoryType;
  }

  /// Check if service is initialized
  bool get isInitialized => _isInitialized;

  /// Get categories by type (for filtering)
  List<CategoryData> getCategoriesByType(CategoryType type) {
    return getAllCategories().where((cat) => cat.categoryType == type).toList();
  }

  /// Search categories by name
  List<CategoryData> searchCategories(String query) {
    if (query.isEmpty) return getAllCategories();

    final lowerQuery = query.toLowerCase();
    return getAllCategories()
        .where((cat) =>
            cat.name.toLowerCase().contains(lowerQuery) ||
            cat.categoryType.name.toLowerCase().contains(lowerQuery))
        .toList();
  }

  /// Force refresh categories (optional, in case of updates)
  Future<void> refresh() async {
    _isInitialized = false;
    _categoriesById = null;
    _allCategories = null;
    await initialize();
  }

  /// Clear cache
  void clear() {
    _isInitialized = false;
    _categoriesById = null;
    _allCategories = null;
    AppLogger.info('CategoryService', 'Cache cleared');
  }
}
