import 'package:famxpense/data/database/repositories/abstractions/i_family_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_user_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_user_repository.dart';
import 'package:famxpense/data/database/repositories/concrete/session_repository.dart';
import 'package:famxpense/domain/entities/family.dart';
import 'package:famxpense/domain/entities/family_user.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'family_state.dart';

class FamilyCubit extends Cubit<FamilyState> {
  final IFamilyRepository _familyRepository;
  final IFamilyUserRepository _familyUserRepository;
  final IUserRepository _userRepository;
  final SessionRepository _sessionRepository;

  String? _lastLoadedUserId;

  FamilyCubit(
    this._familyRepository,
    this._familyUserRepository,
    this._userRepository,
    this._sessionRepository,
  ) : super(const FamilyState());

  Future<void> loadFamilies() async {
    emit(state.copyWith(isLoading: true, error: null));

    try {
      final String? userId = await _sessionRepository.getCurrentUser();
      final user =
          userId != null ? await _userRepository.getById(userId) : null;

      if (user == null) {
        emit(state.copyWith(
          families: <Family>[],
          selectedFamilyId: null,
          isLoading: false,
        ));
        return;
      }

      final visibleFamilies =
          await _familyRepository.getFamiliesForUser(user.id);

      final bool userChanged =
          _lastLoadedUserId != null && _lastLoadedUserId != user.id;
      _lastLoadedUserId = user.id;

      final String? sessionSelectedId =
          await _sessionRepository.getCurrentFamily();

      String? selectedId;

      final bool sessionIdIsValid = sessionSelectedId != null &&
          visibleFamilies.any((f) => f.id == sessionSelectedId);

      if (userChanged) {
        selectedId =
            visibleFamilies.isNotEmpty ? visibleFamilies.first.id : null;
      } else {
        selectedId = state.selectedFamilyId ??
            (sessionIdIsValid ? sessionSelectedId : null) ??
            (visibleFamilies.isNotEmpty ? visibleFamilies.first.id : null);
      }

      if (selectedId != null &&
          (sessionSelectedId == null || sessionSelectedId != selectedId)) {
        await _sessionRepository.setCurrentFamily(selectedId);
      }

      emit(
        state.copyWith(
          families: _sortWithSelected(visibleFamilies, selectedId),
          selectedFamilyId: selectedId,
          isLoading: false,
          error: null,
        ),
      );
    } catch (_) {
      emit(state.copyWith(isLoading: false, error: 'Failed to load families'));
    }
  }

  Future<void> selectFamily(String familyId) async {
    if (state.selectedFamilyId == familyId) return;

    final sorted = _sortWithSelected(state.families, familyId);

    emit(
      state.copyWith(
        selectedFamilyId: familyId,
        families: sorted,
        error: null,
      ),
    );

    await _sessionRepository.setCurrentFamily(familyId);
  }

  Future<void> createFamily({
    required String name,
    required double currentBudget,
  }) async {
    emit(state.copyWith(isSaving: true, error: null));

    try {
      final cleaned = name.trim();
      final displayName =
          cleaned.endsWith("'s Family") ? cleaned : "$cleaned's Family";

      final newFamily = Family.create(
        name: displayName,
        currentBudget: currentBudget,
      );

      await _familyRepository.add(newFamily);

      final userId = await _sessionRepository.getCurrentUser();
      final loggedUser =
          userId != null ? await _userRepository.getById(userId) : null;

      if (loggedUser != null) {
        await _familyUserRepository.add(
          FamilyUser.create(
            familyId: newFamily.id,
            userId: loggedUser.id,
            isParent: true,
            invitedByID: loggedUser.id,
          ),
        );
      }

      final updatedFamilies = List<Family>.from(state.families)..add(newFamily);

      emit(
        state.copyWith(
          isSaving: false,
          families: _sortWithSelected(updatedFamilies, newFamily.id),
          selectedFamilyId: newFamily.id,
          error: null,
        ),
      );

      await _sessionRepository.setCurrentFamily(newFamily.id);
    } catch (_) {
      emit(state.copyWith(isSaving: false, error: 'Failed to create family'));
    }
  }

  void resetOnLogout() {
    _lastLoadedUserId = null;
    emit(const FamilyState());
  }

  List<Family> _sortWithSelected(List<Family> families, String? selectedId) {
    final list = List<Family>.from(families);
    if (selectedId == null) return list;

    list.sort((a, b) {
      if (a.id == selectedId) return -1;
      if (b.id == selectedId) return 1;
      return b.createdAt.compareTo(a.createdAt);
    });

    return list;
  }
}
