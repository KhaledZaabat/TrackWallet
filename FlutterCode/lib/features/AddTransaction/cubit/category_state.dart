import 'package:famxpense/domain/entities/category.dart';

abstract class CategoryState {}

class CategoryInitial extends CategoryState {}

class CategoryLoading extends CategoryState {}

class CategoryLoaded extends CategoryState {
  final List<Category> all;
  final List<Category> filtered;
  final String searchQuery;

  CategoryLoaded({
    required this.all,
    required this.filtered,
    required this.searchQuery,
  });

  CategoryLoaded copyWith({
    List<Category>? all,
    List<Category>? filtered,
    String? searchQuery,
  }) {
    return CategoryLoaded(
      all: all ?? this.all,
      filtered: filtered ?? this.filtered,
      searchQuery: searchQuery ?? this.searchQuery,
    );
  }
}

class CategoryError extends CategoryState {
  final String message;
  CategoryError(this.message);
}
