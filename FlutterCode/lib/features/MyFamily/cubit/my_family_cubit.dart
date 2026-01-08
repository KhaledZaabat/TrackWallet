import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_state.dart';

/// Cubit for managing MyFamily page feature
///
/// Handles all business logic for:
/// - Loading current family details with all members
/// - Displaying family information (name, bio, budget)
/// - Displaying family member list
class MyFamilyCubit extends Cubit<MyFamilyState> {
  final FamilyRepository _repository;

  MyFamilyCubit(this._repository) : super(const MyFamilyInitial());

  /// Load current family details with all members
  ///
  /// Fetches:
  /// - GET /api/families/me: Returns family with all members
  ///
  /// Emits: MyFamilyLoading → MyFamilyLoaded or MyFamilyError
  /// 
  /// Error scenarios:
  /// - 401 Unauthorized: User token expired/invalid
  /// - 404 Not Found: Family doesn't exist (unlikely if route guard works)
  /// - Network Error: Connection failure
  Future<void> loadFamilyDetails() async {
    try {
      emit(const MyFamilyLoading());

      final result = await _repository.getFamilyDetails();

      if (result.isSuccess && result.data != null) {
        emit(MyFamilyLoaded(familyDetails: result.data!));
      } else {
        emit(MyFamilyError(
          result.errorMessage ?? 'Failed to load family details',
        ));
      }
    } catch (e) {
      emit(MyFamilyError(e.toString()));
    }
  }
}
