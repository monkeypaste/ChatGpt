using ChatGpt.Data;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChatGpt {
    public class ChatGptPlugin : MpIAnalyzeComponentAsync, MpISupportHeadlessAnalyzerFormat {
        const string PARAM_ID_MODEL = "model";
        const string PARAM_ID_ORG_ID = "orgid";
        const string PARAM_ID_API_KEY = "apikey";
        const string PARAM_ID_TEMPERATURE = "temp";
        const string PARAM_ID_CONTENT = "content";
        const string PARAM_ID_SIGNUP = "signup";

        const string END_POINT_URL = "https://api.openai.com/v1/chat/completions";

        const string SIGNUP_URL = "https://platform.openai.com/signup";
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            var resp = new MpAnalyzerPluginResponseFormat();
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(
                    new HttpMethod("POST"), END_POINT_URL)) {
                    request.Headers.TryAddWithoutValidation(
                        "Authorization",
                        $"Bearer {req.GetParamValue<string>(PARAM_ID_API_KEY)}");

                    if (req.GetParamValue<string>(PARAM_ID_ORG_ID) is string org_id) {
                        request.Headers.TryAddWithoutValidation("OpenAI-Organization", org_id);
                    }
                    var opai_req = new ChatGptRequest() {
                        model = req.GetParamValue<string>(PARAM_ID_MODEL),
                        temperature = req.GetParamValue<double>(PARAM_ID_TEMPERATURE),
                        messages = new[] {
                            new OpenAiMessage() {
                                role = "user",
                                content = req.GetParamValue<string>(PARAM_ID_CONTENT)
                            }
                        }.ToList()
                    };
                    request.Content = new StringContent(opai_req.SerializeObject());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    try {
                        var http_response = await httpClient.SendAsync(request);
                        string http_response_str = await http_response.Content.ReadAsStringAsync();
                        if (http_response.IsSuccessStatusCode) {
                            var opai_resp = http_response_str.DeserializeObject<ChatGptResponse>();
                            if (opai_resp.choices != null &&
                                opai_resp.choices.FirstOrDefault(x => x.message != null) is { } msg_choice) {
                                resp.dataObjectLookup = new Dictionary<string, object> {
                                    {MpPortableDataFormats.Text, msg_choice.message.content}
                                };
                            }
                        } else {
                            if ((int)http_response.StatusCode == 401) {
                                // invalidate creds
                                resp.invalidParams.Add(PARAM_ID_API_KEY, http_response_str);
                            } else {
                                resp.errorMessage = http_response_str;
                            }
                        }
                    }

                    catch (Exception ex) {
                        resp.errorMessage = ex.Message;
                    }

                }
            }
            return resp;
        }
        public MpAnalyzerComponent GetFormat(MpHeadlessComponentFormatRequest request) {
            Resources.Culture = new System.Globalization.CultureInfo(request.culture);

            return new MpAnalyzerComponent() {
                inputType = new MpPluginInputFormat() {
                    text = true
                },
                outputType = new MpPluginOutputFormat() {
                    text = true
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = Resources.ModelLabel,
                        isRequired = true,
                        controlType = MpParameterControlType.ComboBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        values = new[] {
                            "gpt-4",
                            "gpt-4-32k",
                            "gpt-3.5-turbo",
                            "gpt-3.5-turbo-16k",
                            "dall-e-3"
                        }
                        .Select((x,idx)=>new MpParameterValueFormat(x,idx == 0)).ToList(),
                        paramId = PARAM_ID_MODEL,
                    },
                    new MpParameterFormat() {
                        label = Resources.TempLabel,
                        description = Resources.TempDescription,
                        controlType = MpParameterControlType.Slider,
                        unitType = MpParameterValueUnitType.Decimal,
                        isRequired = true,
                        value = new MpParameterValueFormat(0.75.ToString(),true),
                        paramId = PARAM_ID_TEMPERATURE,
                    },
                    new MpParameterFormat() {
                        isVisible = true,
                        isRequired = true,
                        label = Resources.PromptLabel,
                        description = Resources.PromptDescription,
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        value = new MpParameterValueFormat("{ClipText}",true),
                        paramId = PARAM_ID_CONTENT,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        isSharedValue = true,
                        isRequired = false,
                        label =Resources.OrgLabel,
                        description = Resources.OrgDescription,
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = PARAM_ID_ORG_ID,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        isSharedValue = true,
                        isRequired = true,
                        label = Resources.KeyLabel,
                        description = Resources.KeyDescription,
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = PARAM_ID_API_KEY,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        controlType = MpParameterControlType.Hyperlink,
                        value = new MpParameterValueFormat(SIGNUP_URL,Resources.SignupLabel,true),
                        paramId = PARAM_ID_SIGNUP,
                    },
                }
            };
        }
    }
}
