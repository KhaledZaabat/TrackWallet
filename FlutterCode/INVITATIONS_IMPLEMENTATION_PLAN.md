# Invitations Feature Implementation Plan

## Stack & Architecture Guidelines

**State Management**: BLoC/Cubit with equatable
**UI**: Stateless widgets where possible, minimal logic
**API Client**: Dio with ApiClient wrapper
**Error Handling**: ApiResult<T> pattern (success/failure)
**Data Flow**: UI → Cubit → Repository → ApiClient → Backend

**Key Principles**:
- ✅ Cubit handles ALL business logic (API calls, data transformations)
- ✅ Stateless widgets for presentation only
- ✅ Use BlocConsumer/BlocBuilder for state listening
- ✅ Map API responses to models immediately in repo
- ✅ Handle errors in Cubit, emit InvitationsError state
- ✅ Use SnackBars/Dialogs for user feedback (from UI layer)

---

## Directory Structure
```
lib/features/Invitations/
├── cubit/
│   ├── invitations_cubit.dart
│   └── invitations_state.dart
├── pages/
│   └── invitations_page.dart
└── widgets/
    ├── invitation_card.dart
    ├── send_invitation_dialog.dart
    ├── received_invitations_tab.dart
    └── sent_invitations_tab.dart

lib/data/repos/
├── invitations_repository.dart (new)

lib/models/Invitations/
├── invitation_model.dart (new)
```

---

## Data Models to Create

### `lib/models/Invitations/invitation_model.dart`
Models **MUST match backend API responses exactly**. 

**Invitation Response** (from POST /api/invitations, GET /received, GET /sent):
```json
{
  "invitationId": "uuid",
  "inviteeUserId": "uuid",
  "inviterUserId": "uuid",
  "familyId": "uuid",
  "isParent": true,
  "status": "Pending|Accepted|Declined|Cancelled",
  "sentAtUtc": "2026-01-07T18:40:05.292Z"
}
```

Create:
- **InvitationStatus** enum: `Pending`, `Accepted`, `Declined`, `Cancelled` (case-sensitive!)
- **Invitation** class with fields matching above (all String fields + bool isParent + DateTime sentAtUtc)
- Factory `fromJson()` for API response parsing:
  - Parse `sentAtUtc` using `DateTime.parse(json['sentAtUtc'] as String)` for ISO8601
  - Convert status string to enum: `InvitationStatus.values.firstWhere((s) => s.name == json['status'])`
- Add `toJson()` if needed for requests

**Important Notes**:
- Status values are capital: "Pending", "Accepted", "Declined", "Cancelled"
- Enum names must match exactly
- sentAtUtc is ISO8601 UTC timestamp string from API

---

## Repository Layer

### `lib/data/repos/invitations_repository.dart`
Methods to implement:
```dart
// POST /api/invitations
// Send invitation to user by email
// Returns: Invitation object
Future<ApiResult<Invitation>> sendInvitation({
  required String email,
  required bool isParent,
})

// GET /api/invitations/received
// Get invitations where current user is recipient (all users)
// Optional status filter: Pending, Accepted, Declined, Cancelled
Future<ApiResult<List<Invitation>>> getReceivedInvitations({
  String? status,
})

// GET /api/invitations/sent
// Get invitations sent from current family (parents only - 403 if not parent)
// Optional status filter: Pending, Accepted, Declined, Cancelled
Future<ApiResult<List<Invitation>>> getSentInvitations({
  String? status,
})

// POST /api/invitations/{invitationId}/accept
// Accept a received invitation
// Returns: 200 OK (no body)
Future<ApiResult<void>> acceptInvitation(String invitationId)

// POST /api/invitations/{invitationId}/decline
// Decline a received invitation
// Returns: 200 OK (no body)
Future<ApiResult<void>> declineInvitation(String invitationId)

// POST /api/invitations/{invitationId}/cancel
// Cancel a sent invitation (parents only - 403 if not parent)
// Returns: 200 OK (no body)
Future<ApiResult<void>> cancelInvitation(String invitationId)
```

**Implementation Notes**:
- GET endpoints support optional `status` query parameter
- For GET calls, append `?status=Pending` if status filter needed (not used for now, load all)
- Accept/decline/cancel return void (200 OK with no body)
- Error extraction (follow auth_repository pattern):
  ```dart
  final errorMessage = e.response?.data['detail'] as String? ?? 
                       e.response?.data['message'] as String? ?? 
                       e.message ?? 
                       'An error occurred';
  ```
- Handle 403 Forbidden for parent-only actions:
  - `getSentInvitations`: Show error "You must be a family parent to view sent invitations"
  - `cancelInvitation`: Show error "Only family parents can cancel invitations"
- All methods return ApiResult<T> using DioException handling

---

## State Management (Cubit)

### `lib/features/Invitations/cubit/invitations_state.dart`
States (simplified):
- `InvitationsInitial` - initial state
- `InvitationsLoading` - loading both lists
- `InvitationsLoaded(receivedInvitations, sentInvitations, selectedTab, loadingInvitationId?)` 
  - Both lists loaded
  - `loadingInvitationId`: Optional UUID of invitation being acted upon (used to disable specific card buttons)
  - When loadingInvitationId is set, relevant card shows loading spinner
- `InvitationsError(message)` - error on any operation (load, send, accept, decline, cancel)

Use `equatable` for state comparison.

**State Flow**:
1. Initial → Loading → Loaded
2. User sends/accepts/declines/cancels → emit Loaded with `loadingInvitationId` set
3. API completes → emit Loaded with `loadingInvitationId = null` (refreshed data)
4. Any error → emit Error with message

### `lib/features/Invitations/cubit/invitations_cubit.dart`
**Constructor**: Inject `InvitationsRepository`

**Public Methods** (only 3):
- `loadAll()` - fetch both received + sent, emit Loaded or Error
- `sendInvitation(email, isParent)` - validate, emit Loaded with loadingInvitationId, call repo, then loadAll() on success
- `switchTab(index)` - update selectedTab in state, emit new Loaded state

**Public Action Methods** (called from UI):
- `acceptInvitation(id)` - emit Loaded with loadingInvitationId, call repo, loadAll() on success, emit Error on failure
- `declineInvitation(id)` - emit Loaded with loadingInvitationId, call repo, loadAll() on success, emit Error on failure
- `cancelInvitation(id)` - emit Loaded with loadingInvitationId, call repo, loadAll() on success, emit Error on failure

**Error Handling Pattern** (follow auth_repository.dart):
```dart
try {
  final result = await _repository.methodName();
  if (result.isSuccess) {
    // emit success state or reload
  } else {
    emit(InvitationsError(result.errorMessage ?? 'An error occurred'));
  }
} catch (e) {
  emit(InvitationsError(e.toString()));
}
```

**Action Flow**:
1. User taps Accept/Decline/Cancel
2. UI calls cubit method (e.g., `acceptInvitation(id)`)
3. Cubit emits Loaded with `loadingInvitationId = id` (disables card buttons)
4. Cubit calls repo method
5. On success: calls `loadAll()` to refresh lists (clears loadingInvitationId)
6. On error: emits InvitationsError
7. UI shows snackbar and optional retry button

---

## UI Components

### `lib/features/Invitations/pages/invitations_page.dart`

**Purpose**: Main page for managing invitations with tabbed interface (Received | Sent)
**Type**: Stateless widget (all logic in Cubit)

**Key Implementation Details**:
- **Initialization**:
  - In initState, call `WidgetsBinding.instance.addPostFrameCallback` to load data after first frame
  - Read cubit and call `loadAll()` to fetch both received and sent invitations
  - Only fetch once on page open (no periodic refresh)
- **BlocConsumer**:
  - **listener**: Shows snackbars for success/error
    - InvitationsLoading: Optional loading toast (optional)
    - InvitationsError: Show error snackbar + optional retry action
    - InvitationsLoaded (after action): Show success message like "Invitation accepted!" (check state change)
  - **builder**: Shows loading/error/loaded UI
    - InvitationsLoading: Show centered CircularProgressIndicator or skeleton
    - InvitationsError: Show error card with retry button calling `cubit.loadAll()`
    - InvitationsLoaded: Show Scaffold with TabBar and tabs
- **Scaffold structure**:
  - AppBar: Title "Family Invitations"
  - AppBar.bottom: TabBar with 2 tabs: "Received" (0) and "Sent" (1)
  - Body: TabBarView with corresponding tab widgets
  - FloatingActionButton: "Send Invitation" button → shows SendInvitationDialog with currentUserEmail
- **Error Handling**:
  - If 403 Forbidden (user not parent) for getSentInvitations: Show error "You must be a family parent to view sent invitations"
  - If other error: Show error message + retry button
- **Navigation**:
  - FAB opens dialog with currentUserEmail from SessionRepository
  - On dialog send success: Dialog closes + listener shows snackbar "Invitation sent!"
  - Cubit automatically calls loadAll() on success
- **Tab Persistence**:
  - Keep selectedTab in InvitationsLoaded state
  - Switching tabs just updates state (doesn't reload data)
  - Both lists already loaded and cached in state

**Code Pattern**:
```dart
class InvitationsPage extends StatelessWidget {
  const InvitationsPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<InvitationsCubit, InvitationsState>(
      listener: (context, state) {
        if (state is InvitationsError) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.message),
              action: SnackBarAction(
                label: 'Retry',
                onPressed: () => context.read<InvitationsCubit>().loadAll(),
              ),
            ),
          );
        }
      },
      builder: (context, state) {
        if (state is InvitationsLoading) {
          return Scaffold(
            appBar: AppBar(title: const Text('Family Invitations')),
            body: const Center(child: CircularProgressIndicator()),
          );
        }

        if (state is InvitationsError) {
          return Scaffold(
            appBar: AppBar(title: const Text('Family Invitations')),
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(state.message),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => context.read<InvitationsCubit>().loadAll(),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        }

        if (state is InvitationsLoaded) {
          final currentUserEmail = context.read<SessionRepository>().currentUser?.email ?? '';

          return Scaffold(
            appBar: AppBar(
              title: const Text('Family Invitations'),
              bottom: TabBar(
                onTap: (index) => context.read<InvitationsCubit>().switchTab(index),
                tabs: const [
                  Tab(text: 'Received'),
                  Tab(text: 'Sent'),
                ],
              ),
            ),
            body: TabBarView(
              children: [
                ReceivedInvitationsTab(
                  invitations: state.receivedInvitations,
                  loadingInvitationId: state.loadingInvitationId,
                  onAccept: (id) => context.read<InvitationsCubit>().acceptInvitation(id),
                  onDecline: (id) => context.read<InvitationsCubit>().declineInvitation(id),
                ),
                SentInvitationsTab(
                  invitations: state.sentInvitations,
                  loadingInvitationId: state.loadingInvitationId,
                  onCancel: (id) => context.read<InvitationsCubit>().cancelInvitation(id),
                ),
              ],
            ),
            floatingActionButton: FloatingActionButton(
              onPressed: () {
                showDialog(
                  context: context,
                  builder: (context) => SendInvitationDialog(
                    currentUserEmail: currentUserEmail,
                  ),
                );
              },
              child: const Icon(Icons.mail_outline),
            ),
          );
        }

        return const SizedBox.shrink();
      },
    );
  }
}
```

**Integration with initState**:
```dart
@override
void initState() {
  super.initState();
  WidgetsBinding.instance.addPostFrameCallback((_) {
    context.read<InvitationsCubit>().loadAll();
  });
}
```

### `lib/features/Invitations/widgets/invitation_card.dart`

**Purpose**: Reusable card widget displaying a single invitation with appropriate action buttons
**Props**:
- `Invitation invitation` - the invitation to display
- `VoidCallback? onAccept` - callback when accept button tapped (null if sent tab or non-pending)
- `VoidCallback? onDecline` - callback when decline button tapped (null if sent tab)
- `VoidCallback? onCancel` - callback when cancel button tapped (null if received tab)
- `bool isLoading` - whether this invitation's action is processing (show spinner, disable buttons)

**Key Implementation Details**:
- **Display Information**:
  - From ReceivedInvitationsTab: "Invited by [inviterName] to join [familyName] as [parent/member]"
  - From SentInvitationsTab: "Invited [inviteeEmail] to join [familyName] as [parent/member]"
  - Date: "Sent on [formatted date]"
  - Status badge: Color-coded (Pending=yellow, Accepted=green, Declined=red, Cancelled=gray)
- **Buttons**:
  - ReceivedInvitationsTab: Show "Accept" and "Decline" buttons
  - SentInvitationsTab (Pending): Show "Cancel" button
  - SentInvitationsTab (non-Pending): Show status badge only, no buttons
  - Buttons disabled when isLoading=true
  - Show spinner/loading indicator when isLoading=true
- **Styling**: 
  - Use Card widget with rounded corners
  - Padding for content
  - Hover effect (optional)

**Code Pattern**:
```dart
class InvitationCard extends StatelessWidget {
  final Invitation invitation;
  final VoidCallback? onAccept;
  final VoidCallback? onDecline;
  final VoidCallback? onCancel;
  final bool isLoading;

  const InvitationCard({
    required this.invitation,
    this.onAccept,
    this.onDecline,
    this.onCancel,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    final isSent = onCancel != null || (onAccept == null && onDecline == null);
    final statusColor = _getStatusColor(invitation.status);

    return Card(
      margin: const EdgeInsets.all(8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    isSent
                        ? 'Invited ${invitation.inviteeEmail}'
                        : 'Invited by ${invitation.inviterName}',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                Chip(
                  label: Text(invitation.status.name),
                  backgroundColor: statusColor.withOpacity(0.3),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              'To join ${invitation.familyName} as ${invitation.isParent ? 'parent' : 'member'}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
            const SizedBox(height: 4),
            Text(
              'Sent on ${DateFormat('MMM dd, yyyy').format(invitation.sentAtUtc.toLocal())}',
              style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Colors.grey),
            ),
            const SizedBox(height: 16),
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                if (onAccept != null) ...[
                  ElevatedButton(
                    onPressed: isLoading ? null : onAccept,
                    child: isLoading
                        ? SizedBox(
                            height: 16,
                            width: 16,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Accept'),
                  ),
                  const SizedBox(width: 8),
                ],
                if (onDecline != null) ...[
                  OutlinedButton(
                    onPressed: isLoading ? null : onDecline,
                    child: const Text('Decline'),
                  ),
                  const SizedBox(width: 8),
                ],
                if (onCancel != null && invitation.status == InvitationStatus.pending) ...[
                  OutlinedButton(
                    onPressed: isLoading ? null : onCancel,
                    child: isLoading
                        ? SizedBox(
                            height: 16,
                            width: 16,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Cancel'),
                  ),
                ],
              ],
            ),
          ],
        ),
      ),
    );
  }

  Color _getStatusColor(InvitationStatus status) {
    switch (status) {
      case InvitationStatus.pending:
        return Colors.orange;
      case InvitationStatus.accepted:
        return Colors.green;
      case InvitationStatus.declined:
        return Colors.red;
      case InvitationStatus.cancelled:
        return Colors.grey;
    }
  }
}
```

### `lib/features/Invitations/widgets/send_invitation_dialog.dart`

**Purpose**: Stateful widget for creating and sending new invitations
**Constructor**: `const SendInvitationDialog({required String currentUserEmail})`

**Key Implementation Details**:
- **currentUserEmail parameter**: Passed from page to prevent self-invite validation at UI layer
- **Form structure**:
  - Email TextFormField with FormBuilder:
    - Validator: email format + not equal to currentUserEmail (case-insensitive)
    - Hint: "user@example.com"
    - Clear on changes
  - IsParent toggle (CheckboxListTile):
    - Label: "Invite as family parent"
    - Default: false (invitee as regular member)
- **Send button**:
  - Disabled while loading
  - On tap: Call `context.read<InvitationsCubit>().sendInvitation(email, isParent)`
  - Dialog closes on success (handled by listener in page)
  - Validation runs automatically via FormBuilder
- **Error handling**: Dialog stays open if validation fails, user sees inline validation error
- **Dependencies**: flutter_form_builder, InvitationsCubit

**Code Pattern**:
```dart
class SendInvitationDialog extends StatefulWidget {
  final String currentUserEmail;
  const SendInvitationDialog({required this.currentUserEmail});
  
  @override
  State<SendInvitationDialog> createState() => _SendInvitationDialogState();
}

class _SendInvitationDialogState extends State<SendInvitationDialog> {
  final _formKey = GlobalKey<FormBuilderState>();
  bool _isParent = false;
  
  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Send Family Invitation'),
      content: FormBuilder(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            FormBuilderTextField(
              name: 'email',
              decoration: InputDecoration(
                hint: 'user@example.com',
                labelText: 'Email Address',
              ),
              validator: FormBuilderValidators.compose([
                FormBuilderValidators.email(context),
                (value) {
                  if (value?.toLowerCase() == widget.currentUserEmail.toLowerCase()) {
                    return 'Cannot invite yourself';
                  }
                  return null;
                },
              ]),
            ),
            const SizedBox(height: 16),
            CheckboxListTile(
              title: const Text('Invite as family parent'),
              value: _isParent,
              onChanged: (v) => setState(() => _isParent = v ?? false),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: () {
            if (_formKey.currentState?.saveAndValidate() ?? false) {
              final email = _formKey.currentState?.value['email'] as String;
              context.read<InvitationsCubit>().sendInvitation(email, _isParent);
            }
          },
          child: const Text('Send'),
        ),
      ],
    );
  }
}
```

### `lib/features/Invitations/widgets/received_invitations_tab.dart`

**Purpose**: Display received invitations with Accept/Decline buttons
**Props**:
- `List<Invitation> invitations` - all received invitations to display
- `String? loadingInvitationId` - invitation ID currently being processed (null when idle)
- `Function(String id) onAccept` - callback when accept button tapped
- `Function(String id) onDecline` - callback when decline button tapped

**Key Implementation Details**:
- **Loading State Control**: 
  - Each invitation card receives `isLoading = (loadingInvitationId == invitation.id)`
  - While loading, card's action buttons are disabled and show spinner
  - Only the card being acted on shows loading state; others remain interactive
- **List Display**:
  - ListView or ListView.builder of InvitationCard widgets
  - Filter to show Pending status invitations only
  - Empty state: "No pending invitations"
- **Button Callbacks**: 
  - OnAccept: calls `onAccept(invitation.id)` → page calls cubit method → cubit handles loading
  - OnDecline: calls `onDecline(invitation.id)` → page calls cubit method → cubit handles loading
- **Sorting**: Display newest first (by sentAtUtc descending)

**Code Pattern**:
```dart
class ReceivedInvitationsTab extends StatelessWidget {
  final List<Invitation> invitations;
  final String? loadingInvitationId;
  final Function(String) onAccept;
  final Function(String) onDecline;
  
  const ReceivedInvitationsTab({
    required this.invitations,
    required this.loadingInvitationId,
    required this.onAccept,
    required this.onDecline,
  });

  @override
  Widget build(BuildContext context) {
    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    if (pending.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.mail_outline, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            const Text('No pending invitations'),
          ],
        ),
      );
    }

    return ListView.builder(
      itemCount: pending.length,
      itemBuilder: (context, index) {
        final invitation = pending[index];
        return InvitationCard(
          invitation: invitation,
          isLoading: loadingInvitationId == invitation.id,
          onAccept: () => onAccept(invitation.id),
          onDecline: () => onDecline(invitation.id),
        );
      },
    );
  }
}
```

### `lib/features/Invitations/widgets/sent_invitations_tab.dart`

**Purpose**: Display sent invitations grouped by status with Cancel button for Pending
**Props**:
- `List<Invitation> invitations` - all sent invitations to display
- `String? loadingInvitationId` - invitation ID currently being processed (null when idle)
- `Function(String id) onCancel` - callback when cancel button tapped

**Key Implementation Details**:
- **Loading State Control**: 
  - Each invitation card receives `isLoading = (loadingInvitationId == invitation.id)`
  - While loading, cancel button disabled and shows spinner
  - Only the card being acted on shows loading state
- **List Display**:
  - Show all sent invitations grouped by status (Pending, Accepted, Declined, Cancelled)
  - Optional: Use SliverList with headers for each status group or simple sorting
  - Pending first, then Accepted, Declined, Cancelled
  - Empty state: "You haven't sent any invitations"
- **Cancel Button**: 
  - Only show for Pending status invitations
  - On tap: calls `onCancel(invitation.id)` → page calls cubit method
  - Hidden/disabled for Accepted, Declined, Cancelled
- **Status Display**: 
  - Badge or chip showing current status
  - Color-coded: Pending (yellow), Accepted (green), Declined (red), Cancelled (gray)
- **Sorting**: Within each status group, show newest first (sentAtUtc descending)

**Code Pattern**:
```dart
class SentInvitationsTab extends StatelessWidget {
  final List<Invitation> invitations;
  final String? loadingInvitationId;
  final Function(String) onCancel;
  
  const SentInvitationsTab({
    required this.invitations,
    required this.loadingInvitationId,
    required this.onCancel,
  });

  @override
  Widget build(BuildContext context) {
    if (invitations.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.send_outlined, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            const Text('You haven\'t sent any invitations'),
          ],
        ),
      );
    }

    // Group by status: Pending first, then others
    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));
    
    final others = invitations
        .where((inv) => inv.status != InvitationStatus.pending)
        .toList()
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    return ListView(
      children: [
        if (pending.isNotEmpty) ...[
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text('Pending', style: Theme.of(context).textTheme.titleMedium),
          ),
          ...pending.map((inv) => InvitationCard(
            invitation: inv,
            isLoading: loadingInvitationId == inv.id,
            onCancel: () => onCancel(inv.id),
          )),
        ],
        if (others.isNotEmpty) ...[
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text('Other', style: Theme.of(context).textTheme.titleMedium),
          ),
          ...others.map((inv) => InvitationCard(
            invitation: inv,
            isLoading: loadingInvitationId == inv.id,
          )),
        ],
      ],
    );
  }
}
```

## Integration Points

### 1. Route Addition
- Add to `lib/core/router/routes.dart`: `static const String invitations = '/invitations';`
- Add to `lib/core/router/app_router.dart`: GoRoute for invitations page

### 2. Navigation
- Add to main nav (drawer/bottom nav) or family management flow
- Make accessible after family selection

### 3. Dependency Injection
- Register in `lib/core/di/setup_dependency_injection.dart`:
  ```
  getIt.registerLazySingleton<InvitationsRepository>(...)
  getIt.registerLazySingleton<InvitationsCubit>(...)
  ```

---

## Implementation Order

1. ✅ Create data models (`invitation_model.dart`)
2. ✅ Create repository (`invitations_repository.dart`)
3. ✅ Create state classes (`invitations_state.dart`)
4. ✅ Create cubit (`invitations_cubit.dart`)
5. ✅ Create widgets (cards, dialogs, tabs)
6. ✅ Create main page (`invitations_page.dart`)
7. ✅ Add routes and DI registration
8. ✅ Test and debug

---

## Key Considerations

**Simplified Architecture**:
- Single public method `loadAll()` in Cubit refreshes both received and sent lists
- Action methods (sendInvitation, acceptInvitation, etc.) call loadAll() on success
- LoadingInvitationId field in InvitationsLoaded enables granular button-level loading states (not full-page loading)
- Single InvitationsError state for all failures (no separate ActionError state)

**Error Handling Patterns**:
- Repository catches DioException and extracts message: `response.data['detail']` → `response.data['message']` → `e.message` → fallback
- Cubit emits InvitationsError state with user-friendly message
- UI shows snackbar with error message and optional retry button
- 403 Forbidden for parent-only actions: Show "You must be a family parent to [action]" instead of API error

**Loading State Control**:
- LoadingInvitationId = null when idle, = invitation.id when that invitation's action is processing
- UI passes loadingInvitationId to both tabs
- Tab passes `isLoading = (loadingInvitationId == invitation.id)` to each InvitationCard
- Card disables action buttons and shows spinner only while that specific invitation loads
- Allows user to interact with other invitations while one is loading

**Stateless Widget Pattern**:
- All UI widgets (InvitationsPage, both tabs, InvitationCard, SendInvitationDialog) use stateless + Cubit
- Only SendInvitationDialog is stateful (to manage form state and controllers)
- No local widget state - all logic in Cubit, all data in Cubit state
- BlocConsumer in page listens to state changes and shows snackbars

**Tab Persistence**:
- InvitationsLoaded state includes selectedTab (0 or 1)
- Tab switching calls cubit.switchTab(index) to update state (no reload, just UI update)
- Both lists remain loaded in memory while switching tabs

**List Refresh After Actions**:
- After send/accept/decline/cancel success: Cubit calls loadAll() to refresh both lists
- Ensures UI always shows latest data without stale state
- Better UX than manually updating single item (less error-prone)

**Empty States**:
- ReceivedInvitationsTab: "No pending invitations" with mail_outline icon
- SentInvitationsTab: "You haven't sent any invitations" with send_outlined icon
- Show helpful text + icon, centered on screen

**Status Enum Handling**:
- Backend returns "Pending", "Accepted", "Declined", "Cancelled" (PascalCase)
- Enum names must match exactly (case-sensitive)
- Use `InvitationStatus.values.firstWhere((s) => s.name == json['status'])` for conversion
- Display status with color coding: Pending=yellow, Accepted=green, Declined=red, Cancelled=gray

**DateTime Parsing**:
- API returns ISO8601 format: "2024-01-15T10:30:00Z"
- Parse with: `DateTime.parse(json['sentAtUtc'] as String)`
- Display with: `DateFormat('MMM dd, yyyy').format(invitation.sentAtUtc.toLocal())`

**Parent-Only Features**:
- Check user role before enabling certain features
- GetSentInvitations returns 403 Forbidden if user is not a parent
- CancelInvitation returns 403 Forbidden if user is not a parent
- Handle gracefully: Show error "You must be a family parent to view/cancel invitations"
- If non-parent tries to access sent tab: Show helpful message + option to contact family admin

**Email Validation**:
- Form validates email format (regex or FormBuilder validator)
- Form prevents self-invite: compare with currentUserEmail (passed as parameter, case-insensitive)
- Backend validates that email exists in system (returns 400 Bad Request if not)

**Dialog Design**:
- currentUserEmail passed from page to prevent self-invite at UI layer
- Form stays open on error (validation error or backend error)
- On success: Dialog closes automatically, UI shows snackbar with success message
- Snackbar is shown by listener in page, not dialog itself

**Sorting & Filtering**:
- ReceivedInvitationsTab: Filter to Pending only, sort by sentAtUtc descending (newest first)
- SentInvitationsTab: Show all statuses grouped, sort by sentAtUtc descending within each group
- Optional: Add status filter dropdown to allow filtering by status (for future enhancement)

**No Breaking Changes**:
- Invitations feature is independent of existing auth/transaction/family flows
- Only requires read access to SessionRepository (for currentUserEmail and currentFamilyId)
- No changes to existing models, repositories, or pages

---

## API Endpoints Reference

```
POST   /api/invitations                      → sendInvitation(email, isParent)
                                              Returns: Invitation object
                                              201 Created

GET    /api/invitations/received            → getReceivedInvitations(status?)
                                              All users can access
                                              Returns: List<Invitation>
                                              Query param: status (Pending|Accepted|Declined|Cancelled)

GET    /api/invitations/sent                → getSentInvitations(status?)
                                              Parents only (403 Forbidden for non-parents)
                                              Returns: List<Invitation>
                                              Query param: status (Pending|Accepted|Declined|Cancelled)

POST   /api/invitations/{id}/accept         → acceptInvitation(invitationId)
                                              Returns: void (200 OK)

POST   /api/invitations/{id}/decline        → declineInvitation(invitationId)
                                              Returns: void (200 OK)

POST   /api/invitations/{id}/cancel         → cancelInvitation(invitationId)
                                              Parents only (403 Forbidden for non-parents)
                                              Returns: void (200 OK)
```

---

## Detailed Step-by-Step Implementation

### **Step 1: Create Data Models**
- [ ] Create `lib/models/Invitations/invitation_model.dart`
  - Define `InvitationStatus` enum
  - Create `Invitation` class with all fields
  - Add factory `fromJson()` constructor
  - Add `toJson()` method

**Deliverable**: One file with complete model + serialization

---

### **Step 2: Create Invitations Repository**
- [ ] Create `lib/data/repos/invitations_repository.dart`
  - Inject `ApiClient` in constructor
  - Implement all 6 API methods
  - Handle API responses and error mapping
  - Return `ApiResult<T>` type

**Deliverable**: One repo file with all API integration

---

### **Step 3: Create Invitations State Classes**
- [ ] Create `lib/features/Invitations/cubit/invitations_state.dart`
  - Define abstract `InvitationsState` class
  - Create all 6 state classes:
    - `InvitationsInitial`
    - `InvitationsLoading`
    - `InvitationsLoaded` (with receivedInvitations, sentInvitations, selectedTab)
    - `InvitationsError`
    - `SendingInvitation`
    - `ActionInProgress`

**Deliverable**: One file with all state definitions

---

### **Step 4: Create Invitations Cubit**
- [ ] Create `lib/features/Invitations/cubit/invitations_cubit.dart`
  - Inject `InvitationsRepository`
  - Implement methods:
    - `loadReceivedInvitations()`
    - `loadSentInvitations()`
    - `loadAll()`
    - `sendInvitation(email, isParent)`
    - `acceptInvitation(id)`
    - `declineInvitation(id)`
    - `cancelInvitation(id)`
    - `switchTab(index)`

**Deliverable**: One cubit file with complete logic

---

### **Step 5: Create Invitation Card Widget**
- [ ] Create `lib/features/Invitations/widgets/invitation_card.dart`
  - Build reusable card for displaying invitations
  - Accept invitation data + callbacks
  - Show sender info, family name, date, status
  - Handle loading state for buttons

**Deliverable**: One reusable widget

---

### **Step 6: Create Send Invitation Dialog**
- [ ] Create `lib/features/Invitations/widgets/send_invitation_dialog.dart`
  - Email input field with validation
  - Parent/Child toggle or radio button
  - Send & Cancel buttons
  - Error handling with snackbars
  - Success feedback

**Deliverable**: One dialog widget

---

### **Step 7: Create Received Invitations Tab**
- [ ] Create `lib/features/Invitations/widgets/received_invitations_tab.dart`
  - Display list of received invitations
  - Show Accept & Decline buttons for each
  - Handle empty state message
  - Call cubit methods on button taps

**Deliverable**: One tab widget

---

### **Step 8: Create Sent Invitations Tab**
- [ ] Create `lib/features/Invitations/widgets/sent_invitations_tab.dart`
  - Display list of sent invitations
  - Group or badge by status (Pending/Accepted/Declined)
  - Show Cancel button (only for pending)
  - Handle empty state message

**Deliverable**: One tab widget

---

### **Step 9: Create Main Invitations Page**
- [ ] Create `lib/features/Invitations/pages/invitations_page.dart`
  - Setup TabBar (Received | Sent)
  - BlocProvider for InvitationsCubit
  - BlocConsumer for error handling
  - FAB for "Send Invitation" (show dialog)
  - TabBarView with both tabs
  - Initial data load in initState

**Deliverable**: One complete page with tabs

---

### **Step 10: Register Routes**
- [ ] Update `lib/core/router/routes.dart`
  - Add constant: `static const String invitations = '/invitations';`
  
- [ ] Update `lib/core/router/app_router.dart`
  - Add GoRoute for `/invitations` → `InvitationsPage`
  - Import the page

**Deliverable**: Routes configured

---

### **Step 11: Setup Dependency Injection**
- [ ] Update `lib/core/di/setup_dependency_injection.dart`
  - Register `InvitationsRepository`
  - Register `InvitationsCubit`
  - Ensure proper initialization order

**Deliverable**: DI setup complete

---

### **Step 12: Add Navigation**
- [ ] Update main nav (drawer/bottom nav/app bar)
  - Add link to invitations page
  - Make accessible from family context area
  - Test navigation flow

**Deliverable**: Navigation integrated

---

### **Step 13: Test & Polish**
- [ ] Test all happy paths (send, accept, decline, cancel)
- [ ] Test error cases (network errors, validation failures)
- [ ] Polish UI/UX (loading states, error messages, animations)
- [ ] Verify real-time updates after actions

**Deliverable**: Fully tested feature
