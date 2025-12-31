import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/di/service_locator.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/presentation/family/cubit/manage_family_cubit.dart';
import 'package:famxpense/presentation/family/cubit/manage_family_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class ManageFamilyPage extends StatelessWidget {
  final String familyId;
  const ManageFamilyPage({super.key, required this.familyId});

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();

    return BlocProvider(
      create: (_) => sl<ManageFamilyCubit>()..load(familyId),
      child: HeroControllerScope.none(
        child: Scaffold(
          backgroundColor: const Color(0xFFF5F8FA),
          body: BlocBuilder<ManageFamilyCubit, ManageFamilyState>(
            builder: (context, state) {
              if (state.isLoading) {
                return const Center(child: CircularProgressIndicator());
              }

              if (state.error != null) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(20),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          state.error!,
                          style: const TextStyle(
                            color: Colors.redAccent,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 12),
                        TextButton(
                          onPressed: () => context.pop(),
                          child: const Text('Go back'),
                        )
                      ],
                    ),
                  ),
                );
              }

              final family = state.family;

              return CustomScrollView(
                slivers: [
                  MyAppBar(
                    title: family?.name ?? 'Manage Family',
                    leading: const Icon(Icons.arrow_back),
                    leadingOnPressed: () => context.pop(),
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 18, vertical: 16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          _BudgetCard(
                            budget: state.currentBudget,
                            income: state.incomeTotal,
                            expense: state.expenseTotal,
                            formatter: currency,
                          ),
                          const SizedBox(height: 18),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text(
                                'Members',
                                style: TextStyle(
                                  color: AppColors.mainBlackShade,
                                  fontSize: 16,
                                  fontWeight: FontWeight.w800,
                                ),
                              ),
                              TextButton(
                                onPressed: () {
                                  if (family != null) {
                                    context.push(
                                      Routes.manageFamilyUsers.replaceFirst(
                                        ':id',
                                        family.id,
                                      ),
                                    );
                                  }
                                },
                                child: const Text(
                                  'Manage users',
                                  style: TextStyle(
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.w700,
                                  ),
                                ),
                              )
                            ],
                          ),
                          const SizedBox(height: 10),
                          SizedBox(
                            height: 130,
                            child: ListView.separated(
                              scrollDirection: Axis.horizontal,
                              itemBuilder: (context, index) {
                                final member = state.members[index];
                                return _MemberChip(member: member);
                              },
                              separatorBuilder: (_, __) =>
                                  const SizedBox(width: 12),
                              itemCount: state.members.length,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }
}

class _BudgetCard extends StatelessWidget {
  final double budget;
  final double income;
  final double expense;
  final NumberFormat formatter;

  const _BudgetCard({
    required this.budget,
    required this.income,
    required this.expense,
    required this.formatter,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.stroke, width: 1.2),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 14,
            offset: const Offset(0, 10),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Current Budget',
            style: TextStyle(
              color: AppColors.mainBlackShade,
              fontWeight: FontWeight.w800,
              fontSize: 16,
            ),
          ),
          const SizedBox(height: 10),
          Text(
            formatter.format(budget),
            style: const TextStyle(
              color: AppColors.mainBlackShade,
              fontWeight: FontWeight.w900,
              fontSize: 26,
            ),
          ),
          const SizedBox(height: 14),
          Row(
            children: [
              _StatPill(
                label: 'Income',
                value: formatter.format(income),
                color: const Color(0xFF27AE60),
              ),
              const SizedBox(width: 10),
              _StatPill(
                label: 'Expenses',
                value: formatter.format(expense),
                color: const Color(0xFFE74C3C),
              ),
            ],
          )
        ],
      ),
    );
  }
}

class _StatPill extends StatelessWidget {
  final String label;
  final String value;
  final Color color;

  const _StatPill({
    required this.label,
    required this.value,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Container(
            width: 8,
            height: 8,
            decoration: BoxDecoration(
              color: color,
              shape: BoxShape.circle,
            ),
          ),
          const SizedBox(width: 8),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: TextStyle(
                  color: color,
                  fontWeight: FontWeight.w700,
                  fontSize: 12,
                ),
              ),
              Text(
                value,
                style: TextStyle(
                  color: AppColors.mainBlackShade,
                  fontWeight: FontWeight.w800,
                  fontSize: 14,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _MemberChip extends StatelessWidget {
  final FamilyMember member;
  const _MemberChip({required this.member});

  @override
  Widget build(BuildContext context) {
    final initials = _initials(member.name);
    final hasUrl =
        member.avatarUrl != null && member.avatarUrl!.startsWith('http');
    final badge = member.isParent
        ? const Padding(
            padding: EdgeInsets.only(top: 4),
            child: Text(
              'Parent',
              style: TextStyle(
                color: AppColors.primary,
                fontSize: 11,
                fontWeight: FontWeight.w700,
              ),
            ),
          )
        : const SizedBox.shrink();

    return SizedBox(
      width: 90,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          CircleAvatar(
            radius: 26,
            backgroundColor: AppColors.primary.withValues(alpha: 0.12),
            backgroundImage: hasUrl ? NetworkImage(member.avatarUrl!) : null,
            child: !hasUrl
                ? Text(
                    initials,
                    style: const TextStyle(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w800,
                    ),
                  )
                : null,
          ),
          const SizedBox(height: 6),
          Text(
            member.name,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            textAlign: TextAlign.center,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w700,
              color: AppColors.mainBlackShade,
            ),
          ),
          badge,
        ],
      ),
    );
  }

  String _initials(String name) {
    final parts = name.trim().split(' ');
    if (parts.length == 1) {
      return name.substring(0, 1).toUpperCase();
    }
    return (parts[0].substring(0, 1) + parts[1].substring(0, 1)).toUpperCase();
  }
}
