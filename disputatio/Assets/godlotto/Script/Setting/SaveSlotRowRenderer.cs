using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fungus;

/// <summary>
/// 슬롯 행 텍스트·선택 하이라이트·미리보기 패널·푸터 버튼 활성 상태를 갱신합니다.
/// </summary>
public struct SaveRowWidgets
{
    public int Slot;
    public Button Button;
    public Image Background;
    public TextMeshProUGUI Line1;
    public TextMeshProUGUI Line2;
}

public sealed class SaveSlotRowRenderer
{
    readonly List<SaveRowWidgets> _rows;
    readonly TMP_Text _previewTitle;
    readonly TMP_Text _previewSubtitle;
    readonly TMP_Text _previewMeta;
    readonly Image _previewShot;
    readonly Button _btnSave;
    readonly Button _btnLoad;
    readonly Button _btnDelete;

    public SaveSlotManager SaveSlotManager { get; set; }

    public SaveSlotRowRenderer(
        List<SaveRowWidgets> rows,
        TMP_Text previewTitle,
        TMP_Text previewSubtitle,
        TMP_Text previewMeta,
        Image previewShot,
        Button btnSave,
        Button btnLoad,
        Button btnDelete)
    {
        _rows = rows;
        _previewTitle = previewTitle;
        _previewSubtitle = previewSubtitle;
        _previewMeta = previewMeta;
        _previewShot = previewShot;
        _btnSave = btnSave;
        _btnLoad = btnLoad;
        _btnDelete = btnDelete;
    }

    public void RefreshRowTexts()
    {
        string prefix = SaveSlotManager != null ? SaveSlotManager.SlotKeyPrefix : FungusSaveStorage.DefaultSlotKeyPrefix;
        foreach (SaveRowWidgets w in _rows)
        {
            bool has = SaveSlotManager != null && SaveSlotManager.SlotHasData(w.Slot);
            if (!has)
            {
                w.Line1.text = $"슬롯 {w.Slot} — 빈 슬롯";
                w.Line2.text = "";
                continue;
            }

            FungusSaveSlotSummary.TryReadSlotSummary(w.Slot, prefix, out string scene, out string desc);
            w.Line1.text = string.IsNullOrEmpty(scene) ? $"슬롯 {w.Slot}" : scene;
            w.Line2.text = string.IsNullOrEmpty(desc) ? "저장됨" : desc;
        }
    }

    public void RefreshSelectionVisuals(int selectedSlot)
    {
        foreach (SaveRowWidgets w in _rows)
            w.Background.color = w.Slot == selectedSlot ? SaveBrowserUiBuilder.ColRowSelected : SaveBrowserUiBuilder.ColRow;
    }

    public void RefreshPreview(int selectedSlot)
    {
        string prefix = SaveSlotManager != null ? SaveSlotManager.SlotKeyPrefix : FungusSaveStorage.DefaultSlotKeyPrefix;
        bool has = SaveSlotManager != null && SaveSlotManager.SlotHasData(selectedSlot);
        if (!has)
        {
            _previewTitle.text = $"슬롯 {selectedSlot}";
            _previewSubtitle.text = "빈 슬롯";
            _previewMeta.text = "이 슬롯에 저장하면 현재 진행 상황이 기록됩니다.";
            ClearPreviewImage();
            return;
        }

        FungusSaveSlotSummary.TryReadSlotSummary(selectedSlot, prefix, out string scene, out string desc);
        _previewTitle.text = string.IsNullOrEmpty(scene) ? $"슬롯 {selectedSlot}" : scene;
        _previewSubtitle.text = string.IsNullOrEmpty(desc) ? "" : desc;
        _previewMeta.text = Application.productName;

        string thumbPath = FungusSaveSlotSummary.GetThumbnailPath(selectedSlot, prefix);
        if (File.Exists(thumbPath))
        {
            byte[] bytes = File.ReadAllBytes(thumbPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (tex.LoadImage(bytes))
            {
                if (_previewShot.sprite != null)
                {
                    Object.Destroy(_previewShot.sprite.texture);
                    Object.Destroy(_previewShot.sprite);
                }
                _previewShot.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                _previewShot.color = Color.white;
            }
        }
        else
            ClearPreviewImage();
    }

    public void ClearPreviewImage()
    {
        if (_previewShot.sprite != null)
        {
            Object.Destroy(_previewShot.sprite.texture);
            Object.Destroy(_previewShot.sprite);
            _previewShot.sprite = null;
        }
        _previewShot.color = new Color(0.88f, 0.88f, 0.88f, 1f);
    }

    public void RefreshFooter(int selectedSlot)
    {
        if (SaveSlotManager == null)
            SaveSlotManager = Object.FindFirstObjectByType<SaveSlotManager>(FindObjectsInactive.Include);

        bool has = SaveSlotManager != null && SaveSlotManager.SlotHasData(selectedSlot);
        _btnLoad.interactable = has;
        _btnDelete.interactable = has;
        bool canSave = SaveSlotManager != null && SaveSlotManager.GetResolvedSaveManager() != null;
        _btnSave.interactable = canSave;
    }
}
