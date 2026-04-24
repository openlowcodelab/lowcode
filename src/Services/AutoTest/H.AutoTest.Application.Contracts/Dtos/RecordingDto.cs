using System;
using System.Collections.Generic;
using System.Text;

namespace H.AutoTest.Application.Contracts;


// 请求和响应模型
public class StartRecordingRequest
{
    public string StartUrl { get; set; } = string.Empty;
}

public class StartRecordingResponse
{
    public string SessionId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class StopRecordingRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class StopRecordingResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string RecordedCode { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ParseRecordingRequest
{
    public string RecordedCode { get; set; } = string.Empty;
}

public class ParseRecordingResponse
{
    public List<ProjectCaseStep> Steps { get; set; } = new();
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RecordingStatusResponse
{
    public bool IsRecording { get; set; }
    public string Message { get; set; } = string.Empty;
}