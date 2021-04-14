#include "pch.h"
#include "HL2RmStreamUnityPlugin.h"

#define DBG_ENABLE_VERBOSE_LOGGING 1
#define DBG_ENABLE_INFO_LOGGING 1

extern "C"

using namespace winrt::Windows::Perception::Spatial;


void __stdcall HL2Stream::StartStreaming()
{
#if DBG_ENABLE_INFO_LOGGING
	OutputDebugStringW(L"HL2Stream::StartStreaming: Initializing...\n");
#endif

	SpatialLocator m_locator = SpatialLocator::GetDefault();
	m_worldOrigin = m_locator.CreateStationaryFrameOfReferenceAtCurrentLocation().CoordinateSystem();

	InitializeVideoFrameProcessorAsync();

#if DBG_ENABLE_INFO_LOGGING
	OutputDebugStringW(L"HL2Stream::StartStreaming: Done.\n");
#endif
}

void HL2Stream::StreamingToggle()
{
	m_videoFrameProcessor->StreamingToggle();
}

winrt::Windows::Foundation::IAsyncAction HL2Stream::InitializeVideoFrameProcessorAsync()
{
	if (m_videoFrameProcessorOperation &&
		m_videoFrameProcessorOperation.Status() == winrt::Windows::Foundation::AsyncStatus::Completed)
	{
		return;
	}

	m_videoFrameProcessor = std::make_unique<VideoCameraStreamer>();
	if (!m_videoFrameProcessor.get())
	{
		throw winrt::hresult(E_POINTER);
	}

	co_await m_videoFrameProcessor->InitializeAsync(2000000, m_worldOrigin, L"23940");
}