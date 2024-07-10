using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player weapons...
/// </summary>
public class WeaponManager : Singleton<WeaponManager>
{
    public const int MAX_SLOTS = 3;
    private const float AIM_SPEED = 30f;
    private const float MAX_AIM_ANGLE = 40f;
    private const bool CLAMP_AIM_ANGLES = true;

    private List<Weapon> currentWeapons = new List<Weapon>();
    private PlayerCamera playerCamera;
    private PlayerMovement playerMovement;
    private int selectedSlotIndex = 0;
    private PlayerHUD playerHUD;
    private Player playerEntity;

    private void Awake()
    {
        playerEntity = GetComponentInParent<Player>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        AimParent = transform.Find("AimOrigin");
        WeaponParent = AimParent.Find("WeaponParent");

        // Give the player the starting pistol by default...
        /*PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        Weapon pistolWeapon = prefabDatabase.GetPrefab<Weapon>("Pistol");
        GiveWeapon(pistolWeapon);*/

        // Give the player the shovel by default...
        PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        Weapon shovelWeapon = prefabDatabase.GetPrefab<Weapon>("Shovel");
        GiveWeapon(shovelWeapon);

        UpdateSelectedWeapon();
    }

    private void Update()
    {
        if(IsPlayerArmed && playerEntity.IsAlive)
        {
            UpdateWeaponSelection();
            UpdateWeaponAim();
        }
    }

    private void UpdateWeaponSelection()
    {
        int previousWeaponIndex = selectedSlotIndex;

        // Weapon scrolling...
        if(PlayerInput.WeaponScrollRight)
        {
            if(selectedSlotIndex >= WeaponCount - 1) { selectedSlotIndex = 0; }
            else { selectedSlotIndex++; }
        }
        else if(PlayerInput.WeaponScrollLeft)
        {
            if(selectedSlotIndex <= 0) { selectedSlotIndex = WeaponCount - 1; }
            else { selectedSlotIndex--; }
        }

        // Weapon hotkeys...
        if(PlayerInput.WeaponSlot1 && WeaponCount >= 1) { selectedSlotIndex = 0; }
        if(PlayerInput.WeaponSlot2 && WeaponCount >= 2) { selectedSlotIndex = 1; }
        if(PlayerInput.WeaponSlot3 && WeaponCount >= 3) { selectedSlotIndex = 2; }

        // Set the selected weapon if it's not already selected...
        if(previousWeaponIndex != selectedSlotIndex)
        { 
            // Holster previous weapon...
            currentWeapons[previousWeaponIndex].OnWeaponHolster();
            UpdateSelectedWeapon();
        }
    }

    public bool GiveWeapon(Weapon weaponPrefab, bool autoSelect = true)
    {
        if(IsWeaponAddable(weaponPrefab))
        {
            // Spawn and add to collected weapons...
            Weapon weaponEntity = Instantiate(weaponPrefab, WeaponParent);
            weaponEntity.name = weaponPrefab.name;
            currentWeapons.Add(weaponEntity);

            // Automatically select it...
            if(autoSelect)
            {
                selectedSlotIndex = currentWeapons.IndexOf(weaponEntity);
                UpdateSelectedWeapon();
            }

            return true;
        }

        return false;
    }

    public void RemoveWeapon(int slotIndex, bool selectNextSlot = true)
    {
        // Destroy weapon and remove entry from collected weapons...
        currentWeapons[slotIndex].DestroyEntity();
        currentWeapons.RemoveAt(slotIndex);

        // Select next weapon slot...
        if(selectNextSlot) { SelectNextSlot(); }
    }

    public void ReplaceWeapon(int slotIndex, Weapon weaponPrefab)
    {
        // Destroy weapon and remove entry from collected weapons...
        currentWeapons[slotIndex].DestroyEntity();
        currentWeapons.RemoveAt(slotIndex);

        // Spawn new weapon entity, automatically select it...
        GiveWeapon(weaponPrefab, true);
    }

    private void UpdateWeaponAim()
    {
        Vector2 mousePos = GetMousePos();
        Vector2 aimOrigin = (Vector2)AimParent.position;
        Vector2 aimDir = (mousePos - aimOrigin).normalized;
        float zRot = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        if(CLAMP_AIM_ANGLES)
        {
            zRot = ClampAimAngle(zRot);
        }

        // Calculate rendering layer based on aim angle and weapon type...
        if(zRot >= 0)
        {
            float absZRot = Mathf.Abs(zRot);
            int orderInLayer = absZRot > MAX_AIM_ANGLE - 10f && absZRot < 180f - (MAX_AIM_ANGLE - 10f) ? 1 : 2;
            if(!SelectedWeapon.IsMeleeWeapon) { SelectedWeapon.weaponGFX.sortingOrder = orderInLayer; } // TODO: Remove melee weapon check...
        }
        else
        {
            if(!SelectedWeapon.IsMeleeWeapon) { SelectedWeapon.weaponGFX.sortingOrder = 2; } // TODO: Remove melee weapon check...
        }

        RotateAimParent(zRot);
        AdjustWeaponParentPosition(zRot);
        CheckFlipWeapon(zRot);
    }

    private void UpdateSelectedWeapon()
    {
        if(IsPlayerArmed)
        {
            // Only show the weapon that is currently selected...
            for(int i = 0; i < currentWeapons.Count; i++)
            {
                Weapon weaponEntity = currentWeapons[i];
                weaponEntity.ShowEntity(i == selectedSlotIndex);
            }

            // Set selected weapon...
            SelectedWeapon = currentWeapons[selectedSlotIndex];
            SelectedWeapon.OnWeaponSelected();
            playerHUD.UpdateWeaponSelection(SelectedSlotIndex, WeaponCount, SelectedWeapon.weaponSprite);
        }
    }

    private void SelectNextSlot()
    {
        if(currentWeapons.Count > 0)
        {
            int nextSlotIndex = (selectedSlotIndex + 1) % currentWeapons.Count;
            selectedSlotIndex = nextSlotIndex;
            UpdateSelectedWeapon();
        }
    }

    private float ClampAimAngle(float zRot)
    {
        float maxClamp = MAX_AIM_ANGLE;
        float maxPositiveClamp = 180 + MAX_AIM_ANGLE;
        float maxNegativeClamp = -180 - MAX_AIM_ANGLE;

        if(Mathf.Abs(zRot) > 90)
        {
            if(zRot > 0) { zRot = Mathf.Clamp(zRot, 180 - maxClamp, maxPositiveClamp); }
            else { zRot = Mathf.Clamp(zRot, maxNegativeClamp, -180 + maxClamp); }
        }
        else
        {
            zRot = Mathf.Clamp(zRot, -maxClamp, maxClamp);
        }

        return zRot;
    }

    private void RotateAimParent(float zRot)
    {
        Quaternion targetRotation = Quaternion.Euler(AimParent.rotation.eulerAngles.x, AimParent.rotation.eulerAngles.y, zRot);
        AimParent.rotation = Quaternion.Lerp(AimParent.rotation, targetRotation, AIM_SPEED * Time.deltaTime);
    }

    private void AdjustWeaponParentPosition(float zRot)
    {
        const float LERP_SPEED = 5;
        Vector3 weaponParentLocalPos = WeaponParent.localPosition;
        float xOffset = 0.0f;  // Default offset...

        // Check if the aim angle is closer to the extent of the ClampAimAngle...
        if(Mathf.Abs(zRot) > MAX_AIM_ANGLE - 10f && Mathf.Abs(zRot) < 180f - (MAX_AIM_ANGLE - 10f))
        {
            xOffset = zRot > 0 ? 0.0f : -0.3f;  // Adjust x offset based on the aim direction...
        }

        // Lerp towards the new x offset...
        weaponParentLocalPos.x = Mathf.Lerp(weaponParentLocalPos.x, xOffset, LERP_SPEED * Time.deltaTime);
        WeaponParent.localPosition = weaponParentLocalPos;
    }

    private Vector2 GetMousePos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = playerCamera.Camera.nearClipPlane;
        return playerCamera.Camera.ScreenToWorldPoint(mousePos);
    }

    private void CheckFlipWeapon(float zRot)
    {
        bool flipX = zRot > 90 || zRot < -90;
        AimParent.localScale = new Vector3(1f, flipX ? -1f : 1f, 1f);
        playerMovement.UpdateGFXFlip(flipX);
    }

    public bool IsWeaponAddable(Weapon weaponToAdd)
    {
        return currentWeapons.Count < MAX_SLOTS && !IsWeaponOwned(weaponToAdd.weaponID);
    }

    public bool IsWeaponOwned(string weaponID)
    {
        return currentWeapons.Exists(weapon => weapon.weaponID == weaponID);
    }

    public int WeaponCount { get { return currentWeapons.Count; } }
    public Weapon SelectedWeapon { get; set; } = null;
    public int SelectedSlotIndex { get { return selectedSlotIndex; } }
    public bool IsPlayerArmed { get { return currentWeapons.Count > 0; } }
    public bool IsAttackingAllowed { get; set; } = true;
    public Transform WeaponParent { get; set; } = null;
    public Transform AimParent { get; set; } = null;
}
