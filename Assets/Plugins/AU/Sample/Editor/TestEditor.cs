using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ShowInEditorTest))]
class TestEditor : AU.AUEditor<ShowInEditorTest>
{
    public TestEditor()
    {
        /**
         * Force repaint when force the editor to repaint each editor frame in play mode
         * This is useful when members will be changed frequently 
         * Since the editor will not auto update when the member is not changed by the user
         */
        // forceRepaint = true;

        /**
         * Draw default inspector or not
         */
        drawDefaultFields = false;
    }
}