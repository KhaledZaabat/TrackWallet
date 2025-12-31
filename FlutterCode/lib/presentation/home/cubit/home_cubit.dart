import 'package:famxpense/data/database/repositories/abstractions/i_family_budget_history_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_transaction_repository.dart';
import 'package:famxpense/domain/entities/family_budget_history.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'home_state.dart';

class HomeCubit extends Cubit<HomeState> {
  final IFamilyRepository _familyRepository;
  final IFamilyBudgetHistoryRepository _historyRepository;
  final ITransactionRepository _transactionRepository;

  HomeCubit(
    this._familyRepository,
    this._historyRepository,
    this._transactionRepository,
  ) : super(const HomeState());

  Future<void> loadForFamily(String familyId) async {
    emit(state.copyWith(
      isLoading: true,
      error: null,
      familyId: familyId,
    ));

    try {
      var history = await _historyRepository
          .getHistoryForFamily(familyId);
      // If no history yet, seed with current budget for today so graph is consistent
      if (history.isEmpty) {
        final family =
            await _familyRepository.getById(familyId);
        if (family != null) {
          await _historyRepository.insert(
            FamilyBudgetHistory.create(
              familyId: familyId,
              budget: family.currentBudget,
              date: DateTime.now(),
            ),
          );
          history = await _historyRepository
              .getHistoryForFamily(familyId);
        }
      }
      final txs = await _transactionRepository
          .getByFamily(familyId);

      emit(state.copyWith(
        isLoading: false,
        error: null,
        history: history,
        transactions: txs,
      ));
    } catch (_) {
      emit(state.copyWith(
        isLoading: false,
        error: 'Failed to load home data',
      ));
    }
  }

  Future<void> recordTodayBudget(double budget) async {
    final familyId = state.familyId;
    if (familyId == null) return;
    await _historyRepository.insert(
      FamilyBudgetHistory.create(
        familyId: familyId,
        budget: budget,
      ),
    );
    await loadForFamily(familyId);
  }
}
