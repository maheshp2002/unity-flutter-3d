import 'dart:convert';
import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'package:flutter_unity_widget/flutter_unity_widget.dart';
import 'package:file_picker/file_picker.dart';

class AdminPage extends StatefulWidget {
  @override
  _AdminPageState createState() => _AdminPageState();
}

class _AdminPageState extends State<AdminPage> {
  late UnityWidgetController _unityController;
  bool _isHelpVisible = false;

  final List<String> controls = [
    'W: Move Camera Forward',
    'S: Move Camera Backward',
    'A: Move Camera Left',
    'D: Move Camera Right',
    'Q: Rotate Camera Left',
    'E: Rotate Camera Right',
    'Space: Move Camera Up',
    'Left Shift: Move Camera Down',
    'Z: Rotate Selected Object',
    'Ctrl + Up Arrow: Scale Up Selected Object',
    'Ctrl + Down Arrow: Scale Down Selected Object',
    'Mouse Right Button: Move Selected Object',
    'T: Top View',
    'B: Bottom View',
    'F: Front View',
    'Back: Back View',
    'L: Left View',
    'R: Right View',
    'Delete: Delete Selected Object', // New control for deletion
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Admin 3D Editor'),
      ),
      body: Row(
        children: [
          Expanded(
            child: UnityWidget(
              onUnityCreated: onUnityCreated,
              onUnityMessage: onUnityMessage,
            ),
          ),
          // Help Panel
          AnimatedContainer(
            duration: Duration(milliseconds: 300),
            width: _isHelpVisible ? 300 : 50,
            child: Column(
              children: [
                IconButton(
                  icon: Icon(
                      _isHelpVisible ? Icons.expand_less : Icons.expand_more),
                  onPressed: () {
                    setState(() {
                      _isHelpVisible = !_isHelpVisible;
                    });
                  },
                ),
                if (_isHelpVisible) ...[
                  Expanded(
                    child: Container(
                      padding: EdgeInsets.all(10),
                      color: Colors.grey[200],
                      child: ListView.builder(
                        itemCount: controls.length,
                        itemBuilder: (context, index) {
                          return Text(controls[index]);
                        },
                      ),
                    ),
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
      bottomNavigationBar: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          ElevatedButton(
            onPressed: _importObject,
            child: Text("Import Object"),
          ),
          ElevatedButton(
            onPressed: _addStartPoint,
            child: Text("Add Start Point"),
          ),
          ElevatedButton(
            onPressed: _addEndPoint,
            child: Text("Add Destination"),
          ),
          ElevatedButton(
            onPressed: _addNavigationLine,
            child: Text("Add Navigation Line"),
          ),
          ElevatedButton(
            onPressed: _hideObject,
            child: Text("Hide Object"),
          ),
          ElevatedButton(
            onPressed: _deleteObject,
            child: Text("Delete Object"),
          ),
          ElevatedButton(
            onPressed: _exportScene,
            child: Text("Export Scene"),
          ),
        ],
      ),
    );
  }

  void onUnityCreated(UnityWidgetController controller) {
    _unityController = controller;
  }

  void onUnityMessage(dynamic message) {
    print('Received message from Unity: $message');
  }

  void _importObject() async {
    FilePickerResult? result = await FilePicker.platform.pickFiles(
      type: FileType.custom,
      allowedExtensions: ['obj', 'fbx', 'glb'], // List your supported formats
    );

    if (result != null) {
      Uint8List? fileBytes = result.files.first.bytes;

      if (fileBytes != null) {
        // Send the 3D model file to Unity
        _unityController.postMessage(
            'SceneController', 'Import3DObject', base64Encode(fileBytes));
      } else {
        // Handle the case when fileBytes is null
        print('Failed to retrieve file bytes.');
      }
    } else {
      // User canceled the picker
      print('File picking was canceled.');
    }
  }

  void _addStartPoint() {
    _unityController.postMessage('SceneController', 'AddStartPoint', '');
  }

  void _addEndPoint() {
    _unityController.postMessage('SceneController', 'AddDestinationPoint', '');
  }

  void _addNavigationLine() {
    // Example positions for the navigation line, you might want to customize this
    _unityController.postMessage('SceneController', 'AddNavigationLine',
        '{"start": [0, 0, 0], "end": [1, 0, 1]}');
  }

  void _hideObject() {
    _unityController.postMessage('SceneController', 'HideMeshRenderer', '');
  }

  void _deleteObject() {
    _unityController.postMessage('SceneController', 'DeleteSelectedObject', '');
  }

  void _exportScene() {
    _unityController.postMessage('SceneController', 'ExportScene', 'Downloads');
  }
}
