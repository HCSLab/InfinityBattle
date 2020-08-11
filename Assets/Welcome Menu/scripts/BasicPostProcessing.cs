using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]                         //编辑器状态下可以直接看到运行结果
[RequireComponent (typeof(Camera))]

public class BasicPostProcessing : MonoBehaviour {

	protected void CheckResources()
	{
		bool isSupported = CheckSupport();

		if (isSupported == false)
		{
			NotSupported();
		}
	}

	protected bool CheckSupport()
	{
		if (SystemInfo.supportsImageEffects == false)
		{
			Debug.LogWarning("This platform does not support image effects or render textures.");
			return false;
		}
		return true;
	}

	protected void NotSupported()
	{
		enabled = false;
	}

	void Start () {
		CheckResources();
	}

	//第一个参数指定了特效需要使用的Shader,第二个参数则是用于后期处理的材质。
	protected Material CheckShaderAndCreateMaterial(Shader shader, Material material)
	{
		if(shader == null)
		{
			return null;
		}

		if (shader.isSupported && material && material.shader == shader)
		{
			return material;
		}

		if (!shader.isSupported)
		{
			return null;
		}
		else
		{
			material = new Material(shader);
			material.hideFlags = HideFlags.DontSave;
			if (material)
				return material;
			else
				return null;
		}
	}

	public Shader briSatConShader;
	private Material briSatConMaterial;
	public Material material
	{
		get
		{
			briSatConMaterial = CheckShaderAndCreateMaterial(briSatConShader, briSatConMaterial);
			return briSatConMaterial;
		}
	}

	[Range(0.5f, 2.0f)]
	public float brightness = 1.0f;

	[Range(0.0f, 3.0f)]
	public float saturation = 1.0f;

	[Range(0.5f, 2.0f)]
	public float contrast = 1.0f;

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (material != null)
		{
			material.SetFloat("_Brightness", brightness);
			material.SetFloat("_Saturation", saturation);
			material.SetFloat("_Contrast", contrast);

			Graphics.Blit(src, dest, material);
		}
		else
		{
			Graphics.Blit(src, dest);
		}
	}
}