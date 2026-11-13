using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RPG2D
{
    public static class NpcChatProtocol
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static bool TryCreateRequest(string npcId, string message, IReadOnlyList<NpcChatTurn> history, out string requestId, out string json, out string error)
        {
            requestId = string.Empty;
            json = string.Empty;
            error = string.Empty;
            if (!string.Equals(npcId, NpcChatRules.GuideNpcId, StringComparison.Ordinal))
            {
                error = "该 NPC 不支持 AI 交谈。";
                return false;
            }
            if (!NpcChatRules.IsValidText(message, NpcChatRules.MaxMessageLength))
            {
                error = "输入必须为 1～256 个有效 UTF-16 代码单元，且不能全为空白。";
                return false;
            }
            if (history == null || history.Count > NpcChatRules.MaxHistoryTurns)
            {
                error = "交谈历史必须为最多四个完整问答对。";
                return false;
            }

            var turns = new NpcChatTurnDto[history.Count];
            for (int index = 0; index < history.Count; index++)
            {
                NpcChatTurn turn = history[index];
                if (turn == null || !NpcChatRules.IsValidText(turn.PlayerText, NpcChatRules.MaxMessageLength) || !NpcChatRules.IsValidText(turn.NpcText, NpcChatRules.MaxReplyLength))
                {
                    error = "交谈历史不符合协议。";
                    return false;
                }
                turns[index] = new NpcChatTurnDto { playerText = turn.PlayerText, npcText = turn.NpcText };
            }

            string nextId = Guid.NewGuid().ToString("N");
            string payload = JsonConvert.SerializeObject(new NpcChatRequest { protocolVersion = NpcChatRules.ProtocolVersion, requestId = nextId, npcId = npcId, message = message, history = turns });
            if (StrictUtf8.GetByteCount(payload) > NpcChatRules.MaxRequestBytes)
            {
                error = "交谈请求超过 64 KiB。";
                return false;
            }
            requestId = nextId;
            json = payload;
            return true;
        }

        public static bool TryReadResponse(string json, string expectedRequestId, string expectedNpcId, out NpcChatResponse response, out string error)
        {
            response = null;
            error = "AI 回复不符合协议。";
            if (!NpcChatRules.IsValidRequestId(expectedRequestId) || !string.Equals(expectedNpcId, NpcChatRules.GuideNpcId, StringComparison.Ordinal)) return false;
            if (string.IsNullOrWhiteSpace(json) || json.Length > NpcChatRules.MaxResponseBytes) return false;

            try
            {
                if (StrictUtf8.GetByteCount(json) > NpcChatRules.MaxResponseBytes) return false;
                if (!HasValidUnicodeEscapes(json)) return false;
                using var textReader = new StringReader(json);
                using var reader = new JsonTextReader(textReader) { DateParseHandling = DateParseHandling.None, MaxDepth = 4 };
                JObject root = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read() || root.Properties().Count() != 4) return false;
                if (root["protocolVersion"]?.Type != JTokenType.Integer || root["protocolVersion"].ToString(Formatting.None) != "1") return false;
                if (root["requestId"]?.Type != JTokenType.String || root["npcId"]?.Type != JTokenType.String || root["reply"]?.Type != JTokenType.String) return false;
                _ = JsonUtility.FromJson<NpcChatResponse>(json);
                string requestId = root["requestId"].Value<string>();
                string npcId = root["npcId"].Value<string>();
                string reply = root["reply"].Value<string>();
                if (!string.Equals(requestId, expectedRequestId, StringComparison.Ordinal) || !string.Equals(npcId, expectedNpcId, StringComparison.Ordinal) || !NpcChatRules.IsValidText(reply, NpcChatRules.MaxReplyLength)) return false;
                response = new NpcChatResponse { protocolVersion = NpcChatRules.ProtocolVersion, requestId = requestId, npcId = npcId, reply = reply };
                error = string.Empty;
                return true;
            }
            catch (Exception exception) when (exception is JsonException || exception is ArgumentException || exception is OverflowException)
            {
                return false;
            }
        }

        private static bool HasValidUnicodeEscapes(string json)
        {
            for (int index = 0; index < json.Length; index++)
            {
                if (json[index] != '\\') continue;
                index++;
                if (index >= json.Length) return false;
                if (json[index] != 'u') continue;
                if (index + 4 >= json.Length || !ushort.TryParse(json.Substring(index + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort unit)) return false;
                index += 4;
                if (char.IsLowSurrogate((char)unit)) return false;
                if (!char.IsHighSurrogate((char)unit)) continue;
                if (index + 6 >= json.Length || json[index + 1] != '\\' || json[index + 2] != 'u' || !ushort.TryParse(json.Substring(index + 3, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort low) || !char.IsLowSurrogate((char)low)) return false;
                index += 6;
            }
            return true;
        }
    }
}
