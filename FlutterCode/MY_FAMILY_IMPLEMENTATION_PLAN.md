# MyFamily Page Implementation Plan

## Overview
The MyFamily page displays all members of the currently selected family with their profile information, including names, profile pictures, birthdates, and parent status. This page uses the existing family context and follows the BLoC/Cubit architecture pattern established in the project.

## Architecture Stack
- **State Management**: BLoC Cubit (consistent with Invitations feature)
- **Data Layer**: Family Repository (extend existing)
- **Presentation**: StatefulWidget with conditional navbar (like Dashboard/Invitations)
- **Error Handling**: ApiResult<T> pattern
- **Navigation**: GoRouter with family selection guard

## Current Project Context
- ✅ Family selection already guards route access
- ✅ LocalStorage integration for selectedFamilyId
- ✅ Conditional navbar based on family selection (2-item vs 5-item)
- ✅ Dashboard/Invitations pages already have this pattern
- ✅ ApiClient and Repository pattern established

## API Endpoints

### GET /api/families/me (PRIMARY)
**Purpose**: Fetch current family with all member details
```json
{
  "id": "uuid",
  "name": "string",
  "currentBudget": 0,
  "familyBio": "string",
  "members": [
    {
      "userId": "uuid",
      "fullName": "string",
      "userName": "string",
      "birthDate": "2026-01-08",
      "isMale": true,
      "profileImageUrl": "string",
      "isParent": true
    }
  ]
}
```

### GET /api/families/users (SECONDARY - Optional)
**Purpose**: Simple list of family users (just id + name)
- We'll use /families/me primarily since it has richer data
- /families/users could be used for quick lookups if needed

---

## Phase-by-Phase Implementation

### **Phase 1: Create Data Models**
**Objective**: Define model classes for family and family member data
**Files to Create**:
- `lib/models/Family/family_member_model.dart` - Individual member data
- `lib/models/Family/family_details_model.dart` - Family with members

**Key Requirements**:
- FamilyMember with userId, fullName, userName, birthDate, isMale, profileImageUrl, isParent
- FamilyDetails with id, name, currentBudget, familyBio, members list
- fromJson() factories for API response parsing
- Handle nullable fields (birthDate, bio can be null)
- DateTime parsing for birthDate (DateOnly format from API)

**Why This Phase**: Models must be defined before repository and UI can use them. Clean data contracts prevent rework later.

---

### **Phase 2: Extend Family Repository**
**Objective**: Add getFamilyDetails method to fetch family with members
**File to Modify**:
- `lib/data/repos/family_repository.dart` (already exists)

**Key Requirements**:
- Add method: `Future<ApiResult<FamilyDetails>> getFamilyDetails()`
- Uses GET /api/families/me endpoint
- Returns ApiResult<FamilyDetails> following project pattern
- Handle errors: 401 (unauthorized), 404 (family not found)
- No parameters needed - uses family context from JWT

**Why This Phase**: Repository bridges API and UI. Keeps API calls centralized and testable.

---

### **Phase 3: Create MyFamily Cubit & State**
**Objective**: Manage MyFamily page state and data loading
**Files to Create**:
- `lib/features/MyFamily/cubit/my_family_state.dart` - State definitions
- `lib/features/MyFamily/cubit/my_family_cubit.dart` - Cubit logic

**Key Requirements**:
- States: Initial, Loading, Loaded(familyDetails), Error(message)
- Public method: `loadFamilyDetails()` - fetch family with members
- Handle loading indicator display
- Proper error messages for different error scenarios
- State comparison with equatable

**Why This Phase**: Cubit handles ALL business logic. UI stays clean and testable.

---

### **Phase 4: Create UI Models & Helper Widgets**
**Objective**: Build reusable components for displaying members and family info
**Files to Create**:
- `lib/features/MyFamily/widgets/family_header.dart` - Family name, bio, budget display
- `lib/features/MyFamily/widgets/member_card.dart` - Individual member card
- `lib/features/MyFamily/widgets/members_list.dart` - List of member cards

**Key Requirements**:
- FamilyHeader: Shows family name, currentBudget (formatted currency), familyBio
- MemberCard: Shows profile pic, name, role (Parent/Member), username, birthdate
- MembersListWidget: ListView of member cards, empty state handling
- Consistent styling with existing Dashboard/Invitations pages
- Handle missing profile images with avatar fallback

**Why This Phase**: Reusable components keep code DRY and maintainable.

---

### **Phase 5: Create MyFamily Page**
**Objective**: Assemble page with navbar, header, and member list
**File to Modify**:
- `lib/features/MyFamily/pages/my_family_page.dart` (exists but needs full implementation)

**Key Requirements**:
- StatefulWidget with conditional navbar (2-item vs 5-item based on family selection)
- BlocConsumer for state management
- AppBar with title "Family Members"
- FamilyHeader widget showing family info
- Loading spinner during data fetch
- Error state with retry button
- Members list using MembersListWidget
- initState loads data via cubit.loadFamilyDetails()
- Follow pattern from dashboard_page.dart

**Why This Phase**: Page ties everything together. Finishes user-facing feature.

---

### **Phase 6: Integration & Testing**
**Objective**: Wire up DI, verify routing works, test the complete feature
**Files to Modify**:
- `lib/core/di/setup_dependency_injection.dart` - Register MyFamilyCubit
- Test navigation to MyFamily from navbar
- Test loading states, error handling, member display

**Key Requirements**:
- Register MyFamilyCubit in DI
- Verify route guard redirects correctly
- Test with 0 members (empty state)
- Test with multiple members
- Test with/without profile images
- Test error scenarios (network error, 404, etc.)
- Verify navbar switches correctly when no family selected

**Why This Phase**: Ensures feature works end-to-end and follows project patterns.

---

## File Structure
```
lib/models/Family/
├── family_member_model.dart (new)
├── family_details_model.dart (new)
└── family_models.dart (existing - may import above)

lib/data/repos/
└── family_repository.dart (extend existing)

lib/features/MyFamily/
├── cubit/
│   ├── my_family_cubit.dart (new)
│   └── my_family_state.dart (new)
├── widgets/
│   ├── family_header.dart (new)
│   ├── member_card.dart (new)
│   └── members_list.dart (new)
└── pages/
    └── my_family_page.dart (modify existing)
```

---

## Pattern Reference (From Existing Code)

### State Definition Pattern (from invitations_state.dart)
```dart
abstract class MyFamilyState extends Equatable {
  const MyFamilyState();

  @override
  List<Object?> get props => [];
}

class MyFamilyInitial extends MyFamilyState {
  const MyFamilyInitial();
}

class MyFamilyLoading extends MyFamilyState {
  const MyFamilyLoading();
}

class MyFamilyLoaded extends MyFamilyState {
  final FamilyDetails familyDetails;
  
  const MyFamilyLoaded({required this.familyDetails});

  @override
  List<Object?> get props => [familyDetails];
}

class MyFamilyError extends MyFamilyState {
  final String message;
  
  const MyFamilyError({required this.message});

  @override
  List<Object?> get props => [message];
}
```

### Cubit Pattern (from invitations_cubit.dart)
```dart
class MyFamilyCubit extends Cubit<MyFamilyState> {
  final FamilyRepository _familyRepository;

  MyFamilyCubit(this._familyRepository) : super(const MyFamilyInitial());

  Future<void> loadFamilyDetails() async {
    emit(const MyFamilyLoading());

    try {
      final result = await _familyRepository.getFamilyDetails();
      
      if (result.isSuccess) {
        emit(MyFamilyLoaded(familyDetails: result.value!));
      } else {
        emit(MyFamilyError(message: result.errorMessage ?? 'Failed to load family'));
      }
    } catch (e) {
      emit(MyFamilyError(message: 'An unexpected error occurred'));
    }
  }
}
```

### Page Pattern (from dashboard_page.dart)
```dart
class MyFamilyPage extends StatefulWidget {
  const MyFamilyPage({super.key});

  @override
  State<MyFamilyPage> createState() => _MyFamilyPageState();
}

class _MyFamilyPageState extends State<MyFamilyPage> {
  bool _isFamilySelected = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
          setState(() {
            _isFamilySelected = familyId != null && familyId.isNotEmpty;
          });
        });
        context.read<MyFamilyCubit>().loadFamilyDetails();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Family Members')),
      body: BlocConsumer<MyFamilyCubit, MyFamilyState>(
        listener: (context, state) {
          if (state is MyFamilyError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text(state.message)),
            );
          }
        },
        builder: (context, state) {
          if (state is MyFamilyLoading) {
            return const Center(child: CircularProgressIndicator());
          }
          
          if (state is MyFamilyLoaded) {
            return Column(
              children: [
                FamilyHeader(familyDetails: state.familyDetails),
                MembersListWidget(members: state.familyDetails.members),
              ],
            );
          }
          
          return const SizedBox.shrink();
        },
      ),
      bottomNavigationBar: _buildNavBar(context),
    );
  }

  BottomNavigationBar _buildNavBar(BuildContext context) {
    // Same pattern as dashboard_page
  }
}
```

---

## Error Scenarios

### 401 Unauthorized
- User token expired or invalid
- Redirect to login (handled by route guard)

### 404 Not Found
- Selected family doesn't exist
- Show error: "Family not found"
- Allow user to go back and select different family

### Network Error
- Show error with retry button
- Snackbar message: "Failed to load family members"

---

## Testing Checklist

- [ ] Phase 1: Models serialize/deserialize correctly from API response
- [ ] Phase 2: Repository calls correct endpoint and returns FamilyDetails
- [ ] Phase 3: Cubit transitions through states correctly
- [ ] Phase 4: Widgets display data correctly with proper styling
- [ ] Phase 5: Page loads data on open, navbar switches correctly
- [ ] Phase 6: Feature works end-to-end with various data scenarios

---

## Notes

- **Birthday Display**: Convert `birthDate` (YYYY-MM-DD) to readable format like "Jan 8, 1990" using intl package
- **Parent Badge**: Show "Parent" badge or different color for members with isParent=true
- **Profile Image Fallback**: Show initials avatar if profileImageUrl is null/empty
- **Current Budget**: Format as currency (e.g., $5,000.00) using NumberFormat
- **Family Bio**: If null/empty, show placeholder text like "No family bio yet"
- **Empty Members**: Handle edge case where family has 0 members (unlikely but possible)

---

## Completion Criteria

✅ All 6 phases completed in sequence
✅ Code follows project patterns and conventions
✅ Feature is fully integrated with route guards
✅ Error handling covers all API scenarios
✅ UI is consistent with Dashboard/Invitations styling
✅ Navbar works correctly (conditional 2-item vs 5-item)
✅ Data loads automatically on page open
✅ Code compiles without errors
