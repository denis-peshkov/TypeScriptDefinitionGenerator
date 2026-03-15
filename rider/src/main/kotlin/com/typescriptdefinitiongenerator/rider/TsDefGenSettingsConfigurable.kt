package com.typescriptdefinitiongenerator.rider

import com.intellij.openapi.options.Configurable
import com.intellij.openapi.options.SearchableConfigurable
import com.intellij.ui.components.JBCheckBox
import com.intellij.ui.components.JBTextField
import com.intellij.util.ui.JBUI
import java.awt.BorderLayout
import java.awt.GridBagConstraints
import java.awt.GridBagLayout
import java.awt.Insets
import javax.swing.JComponent
import javax.swing.JLabel
import javax.swing.JPanel
import javax.swing.JSeparator
import javax.swing.JSpinner
import javax.swing.SpinnerNumberModel

class TsDefGenSettingsConfigurable : SearchableConfigurable {

    private var mainPanel: JPanel? = null
    private lateinit var camelCaseEnumerationValues: JBCheckBox
    private lateinit var camelCasePropertyNames: JBCheckBox
    private lateinit var camelCaseTypeNames: JBCheckBox
    private lateinit var webEssentials2015: JBCheckBox
    private lateinit var classInsteadOfInterface: JBCheckBox
    private lateinit var defaultModuleName: JBTextField
    private lateinit var useNamespace: JBCheckBox
    private lateinit var declareModule: JBCheckBox
    private lateinit var ignoreIntellisense: JBCheckBox
    private lateinit var eolType: javax.swing.JComboBox<EOLType>
    private lateinit var indentTab: JBCheckBox
    private lateinit var indentTabSize: JSpinner

    override fun getId(): String = "com.typescriptdefinitiongenerator.rider.TsDefGenSettingsConfigurable"

    override fun getDisplayName(): String = "TypeScript Definition Generator"

    override fun createComponent(): JComponent {
        val settings = TsDefGenSettingsService.getInstance().state
        camelCaseEnumerationValues = JBCheckBox("Camel case enum values", settings.camelCaseEnumerationValues)
        camelCasePropertyNames = JBCheckBox("Camel case property names", settings.camelCasePropertyNames)
        camelCaseTypeNames = JBCheckBox("Camel case type names", settings.camelCaseTypeNames)
        webEssentials2015 = JBCheckBox("Web Essentials 2015 file names (<filename>.cs.d.ts)", settings.webEssentials2015)
        classInsteadOfInterface = JBCheckBox("Class instead of Interface", settings.classInsteadOfInterface)
        defaultModuleName = JBTextField(settings.defaultModuleName, 30)
        useNamespace = JBCheckBox("Use Namespace", settings.useNamespace)
        declareModule = JBCheckBox("Declare module", settings.declareModule)
        ignoreIntellisense = JBCheckBox("Ignore intellisense for client side reference names", settings.ignoreIntellisense)
        eolType = javax.swing.JComboBox<EOLType>(EOLType.entries.toTypedArray()).apply {
            selectedItem = EOLType.entries.find { it.name == settings.eolType } ?: EOLType.LF
        }
        indentTab = JBCheckBox("Indent Tab (use Space when unchecked)", settings.indentTab)
        indentTabSize = JSpinner(SpinnerNumberModel(settings.indentTabSize.coerceIn(1, 8), 1, 8, 1))

        defaultModuleName.toolTipText = "Set the top-level module name for the generated .d.ts file. Default is \"Server.Dtos\""
        useNamespace.toolTipText = "Use Namespace by default, otherwise \"Default Module name\" will be taken."
        classInsteadOfInterface.toolTipText = "Controls whether to generate a class or an interface: default is an Interface"
        declareModule.toolTipText = "Controls whether to generate types in declared module or without one, but with export"
        ignoreIntellisense.toolTipText = "Ignore intellisense for client side reference names"
        indentTab.toolTipText = "Choose indentation to use Tab/Space: default is Tab"
        indentTabSize.toolTipText = "Set amount Spaces to replace the Tab, when Indent Tab is off"
        mainPanel = JPanel(GridBagLayout()).apply {
            val c = GridBagConstraints().apply { fill = GridBagConstraints.HORIZONTAL; weightx = 1.0; insets = Insets(2, 0, 2, 0) }
            fun addRow(comp: JComponent) { c.gridwidth = GridBagConstraints.REMAINDER; add(comp, c) }
            fun addLabeled(label: String, comp: JComponent) {
                c.gridwidth = 1; c.weightx = 0.0; add(JLabel(label), c)
                c.gridwidth = GridBagConstraints.REMAINDER; c.weightx = 1.0; add(comp, c)
            }
            addRow(JLabel("Casing").apply { font = font.deriveFont(java.awt.Font.BOLD) })
            addRow(camelCaseEnumerationValues)
            addRow(camelCasePropertyNames)
            addRow(camelCaseTypeNames)
            addRow(JSeparator())
            addRow(JLabel("Compatibility").apply { font = font.deriveFont(java.awt.Font.BOLD) })
            addRow(webEssentials2015)
            addRow(JSeparator())
            addRow(JLabel("Settings").apply { font = font.deriveFont(java.awt.Font.BOLD) })
            addLabeled("Default Module name:", defaultModuleName)
            addRow(useNamespace)
            addRow(classInsteadOfInterface)
            addRow(declareModule)
            addRow(ignoreIntellisense)
            addLabeled("End Of Line (EOL):", eolType)
            addRow(indentTab)
            addLabeled("Indent Tab Size:", indentTabSize)
            border = JBUI.Borders.empty(10, 10, 10, 10)
        }
        return JPanel(BorderLayout()).apply { add(mainPanel, BorderLayout.CENTER) }
    }

    override fun isModified(): Boolean {
        val s = TsDefGenSettingsService.getInstance().state
        return camelCaseEnumerationValues.isSelected != s.camelCaseEnumerationValues ||
            camelCasePropertyNames.isSelected != s.camelCasePropertyNames ||
            camelCaseTypeNames.isSelected != s.camelCaseTypeNames ||
            webEssentials2015.isSelected != s.webEssentials2015 ||
            classInsteadOfInterface.isSelected != s.classInsteadOfInterface ||
            defaultModuleName.text != s.defaultModuleName ||
            useNamespace.isSelected != s.useNamespace ||
            declareModule.isSelected != s.declareModule ||
            ignoreIntellisense.isSelected != s.ignoreIntellisense ||
            (eolType.selectedItem as EOLType).name != s.eolType ||
            indentTab.isSelected != s.indentTab ||
            (indentTabSize.value as Int) != s.indentTabSize
    }

    override fun apply() {
        val s = TsDefGenSettingsService.getInstance().state
        s.camelCaseEnumerationValues = camelCaseEnumerationValues.isSelected
        s.camelCasePropertyNames = camelCasePropertyNames.isSelected
        s.camelCaseTypeNames = camelCaseTypeNames.isSelected
        s.webEssentials2015 = webEssentials2015.isSelected
        s.classInsteadOfInterface = classInsteadOfInterface.isSelected
        s.defaultModuleName = defaultModuleName.text.trim().ifEmpty { "Server.Dtos" }
        s.useNamespace = useNamespace.isSelected
        s.declareModule = declareModule.isSelected
        s.ignoreIntellisense = ignoreIntellisense.isSelected
        s.eolType = (eolType.selectedItem as EOLType).name
        s.indentTab = indentTab.isSelected
        s.indentTabSize = (indentTabSize.value as Int).coerceIn(1, 8)
    }

    override fun reset() {
        val s = TsDefGenSettingsService.getInstance().state
        camelCaseEnumerationValues.isSelected = s.camelCaseEnumerationValues
        camelCasePropertyNames.isSelected = s.camelCasePropertyNames
        camelCaseTypeNames.isSelected = s.camelCaseTypeNames
        webEssentials2015.isSelected = s.webEssentials2015
        classInsteadOfInterface.isSelected = s.classInsteadOfInterface
        defaultModuleName.text = s.defaultModuleName
        useNamespace.isSelected = s.useNamespace
        declareModule.isSelected = s.declareModule
        ignoreIntellisense.isSelected = s.ignoreIntellisense
        eolType.selectedItem = EOLType.entries.find { it.name == s.eolType } ?: EOLType.LF
        indentTab.isSelected = s.indentTab
        indentTabSize.value = s.indentTabSize
    }

    override fun disposeUIResources() {
        mainPanel = null
    }
}
