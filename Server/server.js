var express = require('express');
var app = express();
var expressWs = require('express-ws')(app);


app.ws('/', function (ws, req) {
    ws.on('message', function (msg) {

        expressWs.getWss().clients.forEach(client => {
            if (client === ws) {
                // this is my inbound connector.  I shouldn't send back anything...
            }
            // resend the message to all clients...
            client.send(msg)
        });

        console.log(msg);
    });
});

app.listen(4000);
console.log("Listening on port 4000 for WS / route");
