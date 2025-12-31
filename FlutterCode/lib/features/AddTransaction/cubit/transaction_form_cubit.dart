import 'package:famxpense/data/database/repositories/abstractions/i_family_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_budget_history_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_transaction_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_user_repository.dart';
import 'package:famxpense/data/database/repositories/concrete/session_repository.dart';
import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/domain/entities/family.dart';
import 'package:famxpense/domain/entities/transaction.dart';
import 'package:famxpense/domain/entities/family_budget_history.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'transaction_form_state.dart';

class TransactionFormCubit extends Cubit<TransactionFormState> {
  final ITransactionRepository transactionRepository;
  final IUserRepository userRepository;
  final IFamilyRepository familyRepository;
  final IFamilyBudgetHistoryRepository historyRepository;
  final SessionRepository session;

  TransactionFormCubit(
    this.transactionRepository,
    this.userRepository,
    this.familyRepository,
    this.historyRepository,
    this.session, {
    Transaction? existing,
    Category? category,
  }) : super(
          TransactionFormState.initial(
            existing: existing,
            category: category,
          ),
        );

  // -------------------------------
  // Helpers
  // -------------------------------

  Future<Family?> _family() async {
    final id = await session.getCurrentFamily();
    return id == null ? null : await familyRepository.getById(id);
  }

  Future<String> _userId() async {
    return (await session.getCurrentUser()) ?? "anonymous";
  }

  void setType(TransactionType type) => emit(state.copyWith(type: type));
  void setDate(DateTime date) => emit(state.copyWith(date: date));
  void setCategory(Category c) => emit(state.copyWith(category: c));

  // -------------------------------
  // VALIDATION
  // -------------------------------

  bool _validate(double amount, String title) {
    if (amount <= 0) {
      emit(state.copyWith(errorMessage: "Amount must be greater than 0"));
      return false;
    }
    if (amount > 9999) {
      emit(state.copyWith(errorMessage: "Amount cannot exceed \$9,999"));
      return false;
    }
    if (title.trim().isEmpty) {
      emit(state.copyWith(errorMessage: "Title cannot be empty"));
      return false;
    }
    return true;
  }

  // -------------------------------
  // SAVE
  // -------------------------------

  Future<void> save({
    required double amount,
    required String title,
    required String notes,
  }) async {
    if (!_validate(amount, title)) return;

    emit(state.copyWith(saving: true, errorMessage: null));

    try {
      final family = await _family();
      if (family == null) {
        emit(state.copyWith(
          saving: false,
          errorMessage: "No family selected",
        ));
        return;
      }

      final createdById = await _userId();

      // NEW TRANSACTION
      if (state.existing == null) {
        final tx = Transaction.create(
          type: state.type,
          amount: amount,
          transactedOn: state.date,
          title: title,
          notes: notes,
          createdByID: createdById,
          familyID: family.id,
          categoryID: state.category?.id ?? "",
        );

        await transactionRepository.add(tx);

        final updatedFamily = family.applyTransaction(tx);
        await familyRepository.update(updatedFamily);

        await historyRepository.insert(
          FamilyBudgetHistory.create(
            familyId: family.id,
            budget: updatedFamily.currentBudget,
            date: DateTime.now(),
          ),
        );

        emit(state.copyWith(saving: false, existing: tx));
        return;
      }

      // UPDATE TRANSACTION
      final oldTx = state.existing!;
      final updatedTx = oldTx.copyWith(
        type: state.type,
        amount: amount,
        transactedOn: state.date,
        title: title,
        notes: notes,
        categoryID: state.category?.id ?? oldTx.categoryID,
      );

      await transactionRepository.update(updatedTx);

      // Recalculate budget
      Family updatedFamily = family.reverseTransaction(oldTx);
      updatedFamily = updatedFamily.applyTransaction(updatedTx);

      await familyRepository.update(updatedFamily);

      await historyRepository.insert(
        FamilyBudgetHistory.create(
          familyId: updatedFamily.id,
          budget: updatedFamily.currentBudget,
          date: DateTime.now(),
        ),
      );

      emit(state.copyWith(saving: false, existing: updatedTx));
    } catch (e) {
      emit(state.copyWith(
        saving: false,
        errorMessage: "Failed to save transaction",
      ));
    }
  }

  // -------------------------------
  // DELETE
  // -------------------------------

  Future<void> delete() async {
    final tx = state.existing;
    if (tx == null) return;

    emit(state.copyWith(deleting: true, errorMessage: null));

    try {
      final family = await _family();
      if (family == null) {
        emit(state.copyWith(
          deleting: false,
          errorMessage: "No family selected",
        ));
        return;
      }

      final updatedFamily = family.reverseTransaction(tx);
      await familyRepository.update(updatedFamily);

      await historyRepository.insert(
        FamilyBudgetHistory.create(
          familyId: updatedFamily.id,
          budget: updatedFamily.currentBudget,
          date: DateTime.now(),
        ),
      );

      await transactionRepository.delete(tx.id);

      emit(state.copyWith(deleting: false, existing: null));
    } catch (_) {
      emit(state.copyWith(
        deleting: false,
        errorMessage: "Failed to delete transaction",
      ));
    }
  }
}
