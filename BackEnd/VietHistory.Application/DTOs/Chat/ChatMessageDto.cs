using System;

namespace VietHistory.Application.DTOs;

public record ChatMessageDto(string Id, string Text, string Sender, DateTime Timestamp);

