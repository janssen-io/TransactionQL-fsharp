import {
  createConnection,
  Diagnostic,
  InitializeParams,
  ProposedFeatures,
  TextDocumentChangeEvent,
  TextDocuments,
  TextDocumentSyncKind,
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import { exec, ExecException } from "child_process";

const connection = createConnection(ProposedFeatures.all);
const documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

connection.onInitialize((_params: InitializeParams) => {
  console.log("TQL | Initializing!");
  return {
    capabilities: {
      textDocumentSync: TextDocumentSyncKind.Incremental,
    },
  };
});

documents.onDidOpen((e: TextDocumentChangeEvent<TextDocument>) => {
  console.log("Content opened: " + e.document.uri);
});
documents.onDidChangeContent(
  (change: TextDocumentChangeEvent<TextDocument>) => {
    console.log("Content changed: " + change.document.version);
    validateChanges(change.document);
  },
);

documents.onDidSave(
  (saved: TextDocumentChangeEvent<TextDocument>) => {
    console.log("Content changed: " + saved.document.version);
    validateTextDocument(saved.document);
  },
);

async function validateChanges(textDocument: TextDocument): Promise<void> {
  console.log('Changes:', textDocument.getText());
}

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
  const diagnostics: Diagnostic[] = [];

  // TODO: don't operatore on saved file...

  // TODO: fix hack for windows
  var uri = textDocument.uri.replace("%3A", ":").replace("file:///", "");
  console.log("TQL | Validating doc | " + uri);
  const tql = exec(
    "tql validate " + uri,
    (err: ExecException | null, stdout: string, stderr: string) => {
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
    },
  );

  await new Promise((resolve) => {
    tql.on("close", resolve);
  }).catch((e) => console.log(e));

  console.log(diagnostics);

  connection.sendDiagnostics({ uri: textDocument.uri, diagnostics });
  console.log("done");
}

documents.listen(connection);
connection.listen();
