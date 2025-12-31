import 'package:famxpense/data/database/repositories/abstractions/i_transaction_repository.dart';
import 'package:famxpense/domain/entities/transaction.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'transactions_state.dart';

class TransactionsCubit extends Cubit<TransactionsState> {
  final ITransactionRepository _transactionRepository;
  TransactionsCubit(this._transactionRepository)
      : super(const TransactionsState());

  Future<void> loadForFamily(String familyId) async {
    emit(state.copyWith(
      isLoading: true,
      error: null,
      familyId: familyId,
    ));
    try {
      final txs = await _transactionRepository.getByFamily(familyId);
      txs.sort(
        (a, b) => b.transactedOn.compareTo(a.transactedOn),
      );
      // Debug: log counts
      // ignore: avoid_print
      print('TransactionsCubit.loadForFamily($familyId) -> ${txs.length} txs');
      final amounts = txs.map((t) => t.amount).toList();
      final minA =
          amounts.isNotEmpty ? amounts.reduce((a, b) => a < b ? a : b) : 0;
      final maxA =
          amounts.isNotEmpty ? amounts.reduce((a, b) => a > b ? a : b) : 0;
      emit(state.copyWith(
        isLoading: false,
        transactions: txs,
        filtered: txs,
        error: null,
        minAmount: 0,
        maxAmount: 9999,
        selectedMinAmount: null,
        selectedMaxAmount: null,
        categoryFilter: {},
        memberFilter: {},
        titleQuery: '',
        notesQuery: '',
        typeFilter: null,
        startDate: null,
        endDate: null,
      ));
    } catch (_) {
      emit(state.copyWith(
        isLoading: false,
        error: 'Failed to load transactions',
      ));
    }
  }

  Future<void> setFilters({
    TransactionType? type,
    Set<String>? categories,
    Set<String>? members,
    double? min,
    double? max,
    String? titleQuery,
    String? notesQuery,
    DateTime? start,
    DateTime? end,
    bool reset = false,
  }) async {
    final familyId = state.familyId;
    if (familyId == null) return;

    final chosenType = reset ? null : type;
    final chosenCategories = reset
        ? <String>{}
        : (categories ?? state.categoryFilter);
    final chosenMembers =
        reset ? <String>{} : (members ?? state.memberFilter);
    final chosenMin =
        reset ? 0.0 : (min ?? state.selectedMinAmount ?? 0.0);
    final chosenMax =
        reset ? 9999.0 : (max ?? state.selectedMaxAmount ?? 9999.0);
    final chosenTitle =
        reset ? '' : (titleQuery ?? state.titleQuery);
    final chosenNotes =
        reset ? '' : (notesQuery ?? state.notesQuery);
    final chosenStart = reset ? null : (start ?? state.startDate);
    final chosenEnd = reset ? null : (end ?? state.endDate);

    final filtered = await _transactionRepository.getFiltered(
      familyId: familyId,
      type: chosenType,
      categoryIds: chosenCategories,
      memberIds: chosenMembers,
      minAmount: chosenMin,
      maxAmount: chosenMax,
      startDate: chosenStart,
      endDate: chosenEnd,
      titleQuery: chosenTitle,
      notesQuery: chosenNotes,
    );
    filtered.sort((a, b) => b.transactedOn.compareTo(a.transactedOn));
    // Debug: log filtered count
    // ignore: avoid_print
    print(
        'TransactionsCubit.setFilters -> filtered ${filtered.length} (type=$chosenType min=$chosenMin max=$chosenMax)');

    emit(state.copyWith(
      typeFilter: chosenType,
      clearTypeFilter: chosenType == null,
      categoryFilter: chosenCategories,
      memberFilter: chosenMembers,
      selectedMinAmount: chosenMin,
      selectedMaxAmount: chosenMax,
      titleQuery: chosenTitle,
      notesQuery: chosenNotes,
      startDate: chosenStart,
      endDate: chosenEnd,
      filtered: filtered,
    ));
  }
}
