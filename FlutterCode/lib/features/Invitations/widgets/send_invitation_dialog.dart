import 'package:flutter/material.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';

/// Stateful widget for creating and sending new family invitations
///
/// This dialog handles:
/// - Email input with validation (format + self-invite prevention)
/// - Parent/Member role selection
/// - Form submission to cubit
/// - Success/error feedback through cubit listener
///
/// The dialog closes automatically on success (handled by page listener)
/// and stays open on validation errors for user correction.
///
/// Constructor parameters:
/// - [currentUserEmail]: Email of the currently logged-in user (for self-invite validation)
/// - [cubit]: InvitationsCubit instance for sending invitations
class SendInvitationDialog extends StatefulWidget {
  final String currentUserEmail;
  final InvitationsCubit cubit;

  const SendInvitationDialog({
    required this.currentUserEmail,
    required this.cubit,
  });

  @override
  State<SendInvitationDialog> createState() => _SendInvitationDialogState();
}

class _SendInvitationDialogState extends State<SendInvitationDialog> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _emailController;
  bool _isParent = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _emailController = TextEditingController();
  }

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  /// Validate email format using basic regex pattern
  String? _validateEmail(String? value) {
    if (value == null || value.isEmpty) {
      return 'Email is required';
    }

    // Basic email validation pattern
    final emailRegex = RegExp(
      r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
    );

    if (!emailRegex.hasMatch(value)) {
      return 'Please enter a valid email address';
    }

    // Prevent self-invite (case-insensitive comparison)
    if (value.toLowerCase() == widget.currentUserEmail.toLowerCase()) {
      return 'Cannot invite yourself';
    }

    return null;
  }

  /// Handle send button press
  void _handleSendInvitation() {
    if (_formKey.currentState!.validate()) {
      final email = _emailController.text.trim();

      // Call cubit to send invitation
      widget.cubit.sendInvitation(email, _isParent);

      // Close dialog on success
      // The page listener will handle showing success snackbar
      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Send Family Invitation'),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Email input field
              TextFormField(
                controller: _emailController,
                decoration: InputDecoration(
                  labelText: 'Email Address',
                  hintText: 'user@example.com',
                  prefixIcon: const Icon(Icons.email_outlined),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                    borderSide: const BorderSide(
                      color: Colors.blue,
                      width: 2,
                    ),
                  ),
                  errorBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                    borderSide: const BorderSide(
                      color: Colors.red,
                      width: 1,
                    ),
                  ),
                ),
                keyboardType: TextInputType.emailAddress,
                validator: _validateEmail,
                textInputAction: TextInputAction.next,
                onChanged: (_) {
                  // Trigger validation on changes
                  _formKey.currentState?.validate();
                },
              ),
              const SizedBox(height: 24),

              // Parent/Member role toggle
              CheckboxListTile(
                title: const Text('Invite as family parent'),
                subtitle: const Text('Parents can manage family invitations'),
                value: _isParent,
                onChanged: (newValue) {
                  setState(() {
                    _isParent = newValue ?? false;
                  });
                },
                contentPadding: EdgeInsets.zero,
                controlAffinity: ListTileControlAffinity.leading,
              ),
            ],
          ),
        ),
      ),
      actions: [
        // Cancel button
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),

        // Send button
        ElevatedButton(
          onPressed: _handleSendInvitation,
          child: const Text('Send'),
        ),
      ],
    );
  }
}
