require 'rubygems'
require 'websocket-client-simple'

ws = WebSocket::Client::Simple.connect 'ws://127.0.0.1:8080'

ws.on :message do |msg|
  puts msg.data
end

ws.on :open do
  puts 'connected to server'
  ws.send 'Client says hello!'
end

ws.on :close do |e|
  p e
  exit 1
end

ws.on :error do |e|
  p e
end

loop do
  ws.send STDIN.gets.strip
end