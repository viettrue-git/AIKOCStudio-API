import { Layout, Typography, Card } from 'antd'

const { Header, Content } = Layout
const { Title, Text } = Typography

function App() {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center' }}>
        <Title level={4} style={{ color: '#F9FAFB', margin: 0 }}>
          AI KOC Studio
        </Title>
      </Header>
      <Content style={{ padding: 32 }}>
        <Card>
          <Title level={3}>Phase 1 scaffold</Title>
          <Text type="secondary">
            Frontend shell placeholder — pages land in later phases.
          </Text>
        </Card>
      </Content>
    </Layout>
  )
}

export default App
