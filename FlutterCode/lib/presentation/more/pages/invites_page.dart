import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/di/service_locator.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_invitation_repository.dart';
import 'package:famxpense/data/database/repositories/concrete/session_repository.dart';
import 'package:famxpense/domain/entities/invitation.dart';
import 'package:famxpense/presentation/family/cubit/family_cubit.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class InvitesPage extends StatefulWidget {
  const InvitesPage({super.key});

  @override
  State<InvitesPage> createState() => _InvitesPageState();
}

class _InvitesPageState extends State<InvitesPage> {
  late final IInvitationRepository _invitationRepository;
  late final SessionRepository _sessionRepository;

  Future<List<Invitation>>? _future;

  @override
  void initState() {
    super.initState();
    _invitationRepository = sl<IInvitationRepository>();
    _sessionRepository = sl<SessionRepository>();
    _load();
  }

  Future<void> _load() async {
    final String? userId = await _sessionRepository.getCurrentUser();
    if (!mounted) return;

    if (userId == null) {
      // match old behaviour: _future == null -> "No user logged in"
      setState(() {
        _future = null;
      });
      return;
    }

    setState(() {
      _future = _invitationRepository.getPendingForUser(userId);
    });
  }

  Future<void> _accept(String id) async {
    await _invitationRepository.accept(id);

    // refresh families for the logged in user
    if (mounted) {
      await context.read<FamilyCubit>().loadFamilies();
    }

    await _load();
  }

  Future<void> _decline(String id) async {
    await _invitationRepository.decline(id);
    await _load();
  }

  @override
  Widget build(BuildContext context) {
    final ThemeData theme = Theme.of(context);

    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      appBar: AppBar(
        title: const Text('My Invites'),
        backgroundColor: Colors.white,
        foregroundColor: AppColors.mainBlackShade,
        elevation: 0,
        centerTitle: true,
      ),
      body: _future == null
          ? const Center(child: Text('No user logged in'))
          : FutureBuilder<List<Invitation>>(
              future: _future,
              builder: (
                BuildContext context,
                AsyncSnapshot<List<Invitation>> snapshot,
              ) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Center(
                    child: CircularProgressIndicator(),
                  );
                }

                final List<Invitation> invites =
                    snapshot.data ?? <Invitation>[];

                if (invites.isEmpty) {
                  return const Center(
                    child: Text(
                      'No pending invites',
                      style: TextStyle(
                        color: AppColors.mainGrayShade,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  );
                }

                return ListView.separated(
                  padding: const EdgeInsets.all(16),
                  itemCount: invites.length,
                  separatorBuilder: (_, __) => const SizedBox(height: 12),
                  itemBuilder: (BuildContext context, int index) {
                    final Invitation inv = invites[index];

                    return Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: AppColors.stroke,
                          width: 1.2,
                        ),
                        boxShadow: <BoxShadow>[
                          BoxShadow(
                            color: Colors.black.withValues(alpha: 0.04),
                            blurRadius: 10,
                            offset: const Offset(0, 6),
                          ),
                        ],
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: <Widget>[
                          Text(
                            'Family: ${inv.familyId}',
                            style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.w800,
                              color: AppColors.mainBlackShade,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            'Invited by: ${inv.inviterUserId}',
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: AppColors.mainGrayShade,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          const SizedBox(height: 12),
                          Row(
                            children: <Widget>[
                              Expanded(
                                child: OutlinedButton(
                                  style: OutlinedButton.styleFrom(
                                    side: BorderSide(
                                      color: Colors.red.shade300,
                                      width: 1.2,
                                    ),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                  ),
                                  onPressed: () => _decline(inv.id),
                                  child: Text(
                                    'Decline',
                                    style: TextStyle(
                                      color: Colors.red.shade400,
                                      fontWeight: FontWeight.w700,
                                    ),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 10),
                              Expanded(
                                child: ElevatedButton(
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: AppColors.primary,
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                  ),
                                  onPressed: () => _accept(inv.id),
                                  child: const Text(
                                    'Accept',
                                    style: TextStyle(
                                      fontWeight: FontWeight.w700,
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    );
                  },
                );
              },
            ),
    );
  }
}
