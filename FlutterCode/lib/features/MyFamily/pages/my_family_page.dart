import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';

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
    // Check if family is selected
    getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
      setState(() {
        _isFamilySelected = familyId != null && familyId.isNotEmpty;
      });
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'My Family',
          style: TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.w700,
            color: Color(0xFF2D3436),
          ),
        ),
        centerTitle: true,
        backgroundColor: Colors.white,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go(Routes.dashboard),
        ),
      ),
      body: const Center(
        child: Text('My Family Page'),
      ),
      bottomNavigationBar: _buildNavBar(context),
    );
  }

  BottomNavigationBar _buildNavBar(BuildContext context) {
    if (_isFamilySelected) {
      return BottomNavigationBar(
        currentIndex: 3,
        onTap: (index) {
          switch (index) {
            case 0:
              context.go(Routes.dashboard);
              break;
            case 1:
              context.go(Routes.invitations);
              break;
            case 2:
              context.go(Routes.transactions);
              break;
            case 3:
              context.go(Routes.myFamily);
              break;
            case 4:
              context.go(Routes.settings);
              break;
          }
        },
        backgroundColor: Colors.white,
        type: BottomNavigationBarType.fixed,
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home),
            label: 'Dashboard',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.mail),
            label: 'Invitations',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.receipt),
            label: 'Transactions',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.people),
            label: 'My Family',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.settings),
            label: 'Settings',
          ),
        ],
      );
    } else {
      return BottomNavigationBar(
        currentIndex: 1,
        onTap: (index) {
          switch (index) {
            case 0:
              context.go(Routes.selectFamily);
              break;
            case 1:
              context.go(Routes.invitations);
              break;
          }
        },
        backgroundColor: Colors.white,
        type: BottomNavigationBarType.fixed,
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.people),
            label: 'Families',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.mail),
            label: 'Invitations',
          ),
        ],
      );
    }
  }
}
