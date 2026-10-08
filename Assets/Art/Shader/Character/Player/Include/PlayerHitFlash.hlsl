//UNITY_SHADER_NO_UPGRADE
#ifndef RPG2D_PLAYER_HIT_FLASH_INCLUDED

#define RPG2D_PLAYER_HIT_FLASH_INCLUDED

void PlayerHitFlash_float(float3 TextureColor, float3 BaseColor, float3 FlashColor, float FlashAmount, out float3 Out)
{
    float3 baseColor = TextureColor * BaseColor;
    Out = lerp(baseColor, FlashColor, saturate(FlashAmount));
}

#endif