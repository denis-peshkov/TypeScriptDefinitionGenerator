package com.typescriptdefinitiongenerator.rider

import com.intellij.openapi.components.PersistentStateComponent
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.State
import com.intellij.openapi.components.Storage

enum class EOLType {
    LF,
    CRLF
}

data class TsDefGenSettingsState(
    var camelCaseEnumerationValues: Boolean = false,
    var camelCasePropertyNames: Boolean = true,
    var camelCaseTypeNames: Boolean = false,
    var webEssentials2015: Boolean = false,
    var classInsteadOfInterface: Boolean = false,
    var defaultModuleName: String = "Server.Dtos",
    var useNamespace: Boolean = true,
    var declareModule: Boolean = true,
    var ignoreIntellisense: Boolean = true,
    var eolType: String = "LF",
    var indentTab: Boolean = true,
    var indentTabSize: Int = 2
)

@Service(Service.Level.APP)
@State(name = "TsDefGenSettings", storages = [Storage("tsdefgen.xml")])
class TsDefGenSettingsService : PersistentStateComponent<TsDefGenSettingsState> {
    private var state = TsDefGenSettingsState()

    override fun getState(): TsDefGenSettingsState = state

    override fun loadState(state: TsDefGenSettingsState) {
        this.state = state
    }

    companion object {
        fun getInstance(): TsDefGenSettingsService =
            com.intellij.openapi.application.ApplicationManager.getApplication().getService(TsDefGenSettingsService::class.java)
    }
}
