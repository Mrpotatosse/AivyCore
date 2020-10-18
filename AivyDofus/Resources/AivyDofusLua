function start_proxy(port)
	return multi_proxy:Active(ProxyCallbackTypeEnum.Dofus2, true, port, 'D:\\AppDofus', 'Dofus')
end

function start_proxy_from_config(config, port)
	return multi_proxy:Active(config.Type, true, port, config.FolderPath, config.ExeName)
end

function get_config(name)
	return multi_proxy._proxy_api:GetData(function(data) return data.Name == name end)
end

-- ProxyEntity * bool * ClientEntity * NetworkElement * NetworkContentElement * uint
function send_message(local_proxy, from_client, client, message, message_content, instance_id)
	if local_proxy == nil then return false
	elseif client == nil then return false
	elseif message == nil then return false
	elseif message_content == nil then return false
	else
		local data = MessageDataBufferWriter(message):Parse(message_content)		
		local final_data_reader = MessageBufferWriter(from_client):Build(message.protocolID, instance_id, data)
		local final_data = final_data_reader.Data
		final_data_reader:Dispose()
		multi_proxy[local_proxy.Port]._client_sender:Handle(client, final_data)
		return true
	end
end

function test_send_chat(proxy, client, channel, content)
	local instance_id = proxy.GLOBAL_INSTANCE_ID + 1
	local message = protocol_dofus2:Get(ProtocolKeyEnum.Messages, function(el) return el.name == 'ChatClientMultiMessage' end)
	local message_content = NetworkContentElement()
	message_content['channel'] = channel
	message_content['content'] = content
	
	if send_message(proxy, true, client, message, message_content, instance_id) then 
		proxy.GLOBAL_INSTANCE_ID = proxy.GLOBAL_INSTANCE_ID + 1	end	
end

-- set comment if you want to start manualy
if proxy == nil then
	config = get_config('updated')
	proxy = start_proxy_from_config(config, 666)
	accept_callback = multi_proxy[proxy.Port]
end
--if proxy == nul then
--	proxy = start_proxy(666)
--	proxy_c = multi_proxy[proxy.Port]
--end

-- remote = proxy_c:_main_remote_client() -> Dofus SERVER -- need to be called when remote was changed (ex: on switch from auth to world on Dofus)
-- local = proxy_c:_main_local_client() -> Dofus CLIENT -- same as remote
-- test_send_chat(proxy, remote, 0, 'walah')