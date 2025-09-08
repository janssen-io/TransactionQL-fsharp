"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const node_1 = require("vscode-languageserver/node");
const vscode_languageserver_textdocument_1 = require("vscode-languageserver-textdocument");
const child_process_1 = require("child_process");
const connection = (0, node_1.createConnection)(node_1.ProposedFeatures.all);
const documents = new node_1.TextDocuments(vscode_languageserver_textdocument_1.TextDocument);
connection.onInitialize((_params) => {
    console.log("TQL | Initializing!");
    return {
        capabilities: {
            textDocumentSync: node_1.TextDocumentSyncKind.Full,
        },
    };
});
connection.onInitialized(() => {
    console.log("Initialized :)");
});
documents.onDidOpen((e) => {
    console.log("Content opened: " + e.document.uri);
});
documents.onDidChangeContent((change) => {
    console.log("Content changed: " + change.document.version);
    validateTextDocument(change.document);
});
async function validateTextDocument(textDocument) {
    const diagnostics = [];
    // TODO: don't operatore on saved file...
    // TODO: fix hack for windows
    var uri = textDocument.uri.replace("%3A", ":").replace("file:///", "");
    console.log("TQL | Validating doc | " + uri);
    const tql = (0, child_process_1.exec)("tql validate " + uri, (err, stdout, stderr) => {
        console.log("validated | ", err, stdout, stderr);
        if (err && !stdout) {
            diagnostics.push({
                message: `${err.message}`,
                source: "tql cli",
                range: {
                    start: textDocument.positionAt(0),
                    end: textDocument.positionAt(1),
                },
                severity: 2,
            });
        }
        if (stdout) {
            const lines = stdout.trim().split(/\n|\r\n/);
            const hasDigits = Array.from(lines[0].matchAll(/\d+/g));
            if (hasDigits) {
                const [line, col] = hasDigits.map((s) => +s[0]);
                console.log(line, col, hasDigits);
                const label = lines[lines.length - 1];
                diagnostics.push({
                    message: label,
                    range: {
                        start: { line: line - 1, character: col - 1 },
                        end: { line: line - 1, character: col },
                    },
                    source: "tql cli",
                    severity: 1,
                });
            }
        }
    });
    await new Promise((resolve) => {
        tql.on("close", resolve);
    }).catch((e) => console.log(e));
    console.log(diagnostics);
    connection.sendDiagnostics({ uri: textDocument.uri, diagnostics });
    console.log("done");
}
documents.listen(connection);
connection.listen();
//# sourceMappingURL=server.js.map