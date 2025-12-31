import 'package:famxpense/data/database/repositories/abstractions/i_family_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_user_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_invitation_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_transaction_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_user_repository.dart';

import 'package:famxpense/domain/entities/invitation.dart';
import 'package:famxpense/domain/entities/transaction.dart';
import 'package:famxpense/domain/entities/user.dart';
import 'package:famxpense/domain/entities/family_user.dart';
import 'package:famxpense/domain/entities/family.dart';

import 'package:famxpense/data/database/repositories/concrete/session_repository.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'manage_family_state.dart';

class ManageFamilyCubit extends Cubit<ManageFamilyState> {
  final IFamilyRepository _familyRepository;
  final ITransactionRepository _transactionRepository;
  final IFamilyUserRepository _familyUserRepository;
  final IUserRepository _userRepository;
  final IInvitationRepository _invitationRepository;
  final SessionRepository _sessionRepository;

  ManageFamilyCubit(
    this._familyRepository,
    this._transactionRepository,
    this._familyUserRepository,
    this._userRepository,
    this._invitationRepository,
    this._sessionRepository,
  ) : super(const ManageFamilyState());

  Future<void> load(String familyId) async {
    emit(state.copyWith(isLoading: true, error: null));

    try {
      final Family? family = await _familyRepository.getById(familyId);

      if (family == null) {
        emit(state.copyWith(
          isLoading: false,
          error: 'Family not found',
        ));
        return;
      }

      final transactions = await _transactionRepository.getByFamily(familyId);
      final totals = _computeTotals(transactions);
      final members = await _loadMembers(familyId);

      emit(
        state.copyWith(
          isLoading: false,
          family: family,
          incomeTotal: totals.$1,
          expenseTotal: totals.$2,
          members: members,
        ),
      );
    } catch (_) {
      emit(state.copyWith(
        isLoading: false,
        error: 'Failed to load family overview',
      ));
    }
  }

  (double income, double expense) _computeTotals(
      List<Transaction> transactions) {
    double income = 0;
    double expense = 0;

    for (final t in transactions) {
      if (t.type == TransactionType.income) {
        income += t.amount;
      } else {
        expense += t.amount;
      }
    }

    return (income, expense);
  }

  Future<List<FamilyMember>> _loadMembers(String familyId) async {
    try {
      final List<FamilyUser> famUsers =
          await _familyUserRepository.getByFamily(familyId);

      if (famUsers.isEmpty) {
        return _buildFallbackMembers();
      }

      final List<User?> users = await Future.wait(
        famUsers.map((fu) => _userRepository.getById(fu.userId)),
      );

      return List.generate(famUsers.length, (i) {
        final fu = famUsers[i];
        final user = users[i];

        return FamilyMember(
          id: fu.userId,
          name: user?.fullName ?? "Member ${fu.userId.substring(0, 4)}",
          avatarUrl: user?.profilePictureUrl,
          isParent: fu.isParent,
        );
      });
    } catch (_) {
      return _buildFallbackMembers();
    }
  }

  Future<void> inviteByEmail(
    String email, {
    bool isParent = false,
  }) async {
    final family = state.family;
    if (family == null) return;

    final trimmedEmail = email.trim();
    if (trimmedEmail.isEmpty || !trimmedEmail.contains("@")) {
      emit(state.copyWith(error: "Enter a valid email address"));
      return;
    }

    try {
      final User? existingUser =
          await _userRepository.findByEmail(trimmedEmail);

      if (existingUser == null) {
        emit(state.copyWith(error: "User with this email does not exist"));
        return;
      }

      // ✔ User exists, create invitation
      final currentUserId = await _sessionRepository.getCurrentUser();

      final invitation = Invitation.create(
        inviteeUserId: existingUser.id,
        inviterUserId: currentUserId ?? "system",
        familyId: family.id,
        isParent: isParent,
      );

      await _invitationRepository.send(invitation);

      emit(state.copyWith(error: null));
    } catch (_) {
      emit(state.copyWith(error: "Failed to invite user"));
    }
  }

  String _nameFromEmail(String email) {
    final local = email.split("@").first;
    if (local.isEmpty) return "Member";
    return local[0].toUpperCase() + local.substring(1);
  }

  Future<List<FamilyMember>> _buildFallbackMembers() async {
    final currentUserId = await _sessionRepository.getCurrentUser();
    final currentUser = currentUserId != null
        ? await _userRepository.getById(currentUserId)
        : null;

    if (currentUser != null) {
      return [
        FamilyMember(
          id: currentUser.id,
          name: currentUser.fullName,
          avatarUrl: currentUser.profilePictureUrl,
          isParent: true,
        ),
      ];
    }

    return const [
      FamilyMember(
        id: "placeholder",
        name: "Family Member",
        isParent: true,
        avatarUrl: null,
      ),
    ];
  }
}
