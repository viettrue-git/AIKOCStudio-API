import { theme, type ThemeConfig } from 'antd'

/**
 * Ant Design theme tokens extracted from the project's UI mockup.
 * Source: docs/ui-design-system-tokens-from-claude-mockup.md
 */
export const antDesignDarkTheme: ThemeConfig = {
  algorithm: theme.darkAlgorithm,
  token: {
    colorPrimary: '#4F7CFF',
    colorBgBase: '#07090F',
    colorBgContainer: 'rgba(17,24,39,0.7)',
    colorBgElevated: '#0B0F19',
    colorBgLayout: '#07090F',
    colorBorder: 'rgba(255,255,255,0.08)',
    colorBorderSecondary: 'rgba(255,255,255,0.06)',
    colorText: '#E5E7EB',
    colorTextSecondary: '#9CA3AF',
    colorTextTertiary: '#6B7280',
    colorSuccess: '#10B981',
    colorWarning: '#F59E0B',
    borderRadius: 12,
    fontFamily: "'Inter', system-ui, sans-serif",
  },
}
