## What changed
- Summary:

## Inventor add-in checklist
- [ ] `.addin` manifest loads and GUIDs are unique.
- [ ] `ApplicationAddInServer.Activate/Deactivate` cleanly attach and release UI/events.
- [ ] No `Documents.Open` in traversal. Use `AllLeafOccurrences`.
- [ ] Sheet-metal detection uses `SheetMetalComponentDefinition`.
- [ ] Read-only and Content Center parts route to temp-copy export if needed.
- [ ] DXF translator string is documented in code and README.
- [ ] No COM objects left undisposed. No hidden modal UI.
- [ ] Long loops have cancellation and logging.

## Test notes
- Doc types tested: Part, Assembly with sub-assemblies.
- Model states and suppression respected.
- Screenshots or logs attached.

## Risks
- Known limitations:
