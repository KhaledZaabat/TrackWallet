import 'package:famxpense/data/database/repositories/abstractions/i_category_repository.dart';
import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/features/AddTransaction/cubit/category_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class CategoryCubit extends Cubit<CategoryState> {
  final ICategoryRepository categoryRepository;

  CategoryCubit(this.categoryRepository) : super(CategoryInitial());

  Future<void> loadCategories() async {
    emit(CategoryLoading());
    try {
      final List<Category> categories = await categoryRepository.getAll();
      emit(CategoryLoaded(
        all: categories,
        filtered: categories,
        searchQuery: '',
      ));
    } catch (_) {
      emit(CategoryError('Failed to load categories'));
    }
  }

  void search(String query) {
    if (state is! CategoryLoaded) return;

    final CategoryLoaded current = state as CategoryLoaded;
    final String q = query.trim().toLowerCase();

    if (q.isEmpty) {
      emit(current.copyWith(filtered: current.all));
      return;
    }

    final List<Category> filtered = current.all
        .where((c) => c.type.name.toLowerCase().contains(q))
        .toList();

    emit(current.copyWith(filtered: filtered, searchQuery: query));
  }
}
