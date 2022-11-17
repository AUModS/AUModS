using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace AUMod {
public class CustomButton {
    public static List<CustomButton> buttons = new List<CustomButton>();
    public ActionButton actionButton;
    public float MaxTimer = float.MaxValue;
    public float Timer = 0f;
    private Action OnClick;
    private Action OnMeetingEnds;
    private Func<bool> HasButton;
    private Func<bool> CouldUse;
    private Action OnEffectEnds;
    public bool HasEffect;
    public bool isEffectActive = false;
    public bool skipSetCoolDown = false;
    public float EffectDuration;
    public Sprite sprite;
    public Vector3 positionOffset = new Vector3(0, 0, 0);
    public bool positionTransform = false;
    private HudManager hudManager;
    private KeyCode? hotkey;

    public CustomButton(
        Action OnClick,
        Func<bool> HasButton,
        Func<bool> CouldUse,
        Action OnMeetingEnds,
        ActionButton baseButton,
        HudManager hudManager,
        KeyCode? hotkey,
        bool HasEffect,
        float EffectDuration,
        Action OnEffectEnds)
    {
        this.hudManager = hudManager;
        this.OnClick = OnClick;
        this.HasButton = HasButton;
        this.CouldUse = CouldUse;
        this.OnMeetingEnds = OnMeetingEnds;
        this.HasEffect = HasEffect;
        this.EffectDuration = EffectDuration;
        this.OnEffectEnds = OnEffectEnds;
        this.hotkey = hotkey;
        Timer = 16.2f;
        buttons.Add(this);
        actionButton = UnityEngine.Object.Instantiate(baseButton, hudManager.transform);
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);

        setActive(false);
    }

    public CustomButton(
        Action OnClick,
        Func<bool> HasButton,
        Func<bool> CouldUse,
        Action OnMeetingEnds,
        ActionButton baseButton,
        HudManager hudManager,
        KeyCode? hotkey)
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, baseButton, hudManager, hotkey, false, 0f, () => {})
    {
    }

    void onClickEvent()
    {
        if (this.Timer < 0f && HasButton() && CouldUse()) {
            this.OnClick();

            actionButton.SetDisabled();

            if (this.HasEffect && !this.isEffectActive) {
                this.Timer = this.EffectDuration;
                actionButton.SetEnabled();
                this.isEffectActive = true;
            }
        }
    }

    public static void HudUpdate()
    {
        buttons.RemoveAll(item => item.actionButton == null);

        for (int i = 0; i < buttons.Count; i++) {
            try {
                buttons[i].Update();
            } catch (NullReferenceException) {
                System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    public static void MeetingEndedUpdate()
    {
        buttons.RemoveAll(item => item.actionButton == null);
        for (int i = 0; i < buttons.Count; i++) {
            try {
                buttons[i].OnMeetingEnds();
                buttons[i].Update();
            } catch (NullReferenceException) {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    public static void ResetAllCooldowns()
    {
        for (int i = 0; i < buttons.Count; i++) {
            try {
                buttons[i].Timer = buttons[i].MaxTimer;
                buttons[i].Update();
            } catch (NullReferenceException) {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    public void setActive(bool isActive)
    {
        if (isActive)
            actionButton.Show();
        else
            actionButton.Hide();
    }

    private void Update()
    {
        if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton()) {
            setActive(false);
            return;
        }

        setActive(true);

        if (sprite != null)
            actionButton.graphic.sprite = sprite;
        if (hudManager.UseButton != null && positionTransform) {
            Vector3 pos = hudManager.UseButton.transform.localPosition;
            actionButton.transform.localPosition = pos + positionOffset;
        }

        if (CouldUse()) {
            actionButton.SetEnabled();
        } else {
            actionButton.SetDisabled();
        }

        if (Timer >= 0) {
            if (HasEffect && isEffectActive)
                Timer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                Timer -= Time.deltaTime;
        }

        // using
        if (Timer <= 0 && HasEffect && isEffectActive) {
            isEffectActive = false;
            actionButton.SetEnabled();
            OnEffectEnds();
        }

        // workaround
        if (!skipSetCoolDown)
            actionButton.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);

        // Trigger OnClickEvent if the hotkey is being pressed down
        if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value))
            onClickEvent();
    }
}
}
