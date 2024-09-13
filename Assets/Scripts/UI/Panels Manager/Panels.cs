using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Panels References")]
public class Panels : SingleScriptable<Panels>
{

    [SerializeField] private Prefab<UI_InventorySelectScreen> _selectionScreen;
    public static Prefab<UI_InventorySelectScreen> SelectionScreen => Instance._selectionScreen;

    [SerializeField] private Prefab<UI_YesNoWindow> _confirmationScreen;
    public static Prefab<UI_YesNoWindow> ConfirmationScreen => Instance._confirmationScreen;


}
