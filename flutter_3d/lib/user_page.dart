import 'package:flutter/material.dart';
import 'package:flutter_unity_widget/flutter_unity_widget.dart';
import 'package:qr_code_scanner/qr_code_scanner.dart';

class UserPage extends StatefulWidget {
  @override
  _UserPageState createState() => _UserPageState();
}

class _UserPageState extends State<UserPage> {
  final GlobalKey qrKey = GlobalKey(debugLabel: 'QR');
  QRViewController? _qrController;
  String? _qrData;
  late UnityWidgetController _unityController;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('User Navigation'),
      ),
      body: Column(
        children: [
          // QR Code Scanner
          Expanded(
            flex: 2,
            child: QRView(
              key: qrKey,
              onQRViewCreated: _onQRViewCreated,
            ),
          ),
          // Unity AR Navigation View
          Expanded(
            flex: 3,
            child: UnityWidget(
              onUnityCreated: _onUnityCreated,
            ),
          ),
          // Navigation Control Buttons
          if (_qrData != null)
            ElevatedButton(
              onPressed: () {
                // Start AR navigation after scanning QR
                _startNavigation();
              },
              child: Text('Start Navigation'),
            ),
        ],
      ),
    );
  }

  void _onUnityCreated(UnityWidgetController controller) {
    _unityController = controller;
  }

  void _onQRViewCreated(QRViewController controller) {
    setState(() {
      _qrController = controller;
    });

    controller.scannedDataStream.listen((scanData) {
      setState(() {
        _qrData = scanData.code; // Save scanned QR data
      });
    });
  }

  void _startNavigation() {
    // Send the QR data (destination) to Unity for navigation
    _unityController.postMessage('ARController', 'StartNavigation', _qrData!);
  }

  @override
  void dispose() {
    _qrController?.dispose();
    super.dispose();
  }
}
