pipeline {
    agent any
    
    environment {
        scannerHome = tool name: 'sonar_scanner_dotnet'
        username = 'aakashgarg'
        registry = 'iaakashgarg/app-test'
        docker_port = "${env.BRANCH_NAME == "master" ? "7200" : "7300"}"
		k8_port = "${env.BRANCH_NAME == "master" ? "30157" : "30158"}"
		CONTAINER_ID = null
		deployment_name = "app-${username}-${BRANCH_NAME}-deployment"
		service_name = "app-${username}-${BRANCH_NAME}-service"
    }
    
    tools {
		  msbuild 'MSBuild'
	  }

    
    options {
        timestamps()
        timeout(time: 1, unit: 'HOURS')
        buildDiscarder logRotator(artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '10', numToKeepStr: '20')
    }
    
    stages {
		
		stage('nuget restore') {
			steps {
				echo "Start restoring packages"
				bat "dotnet restore WebApplication4\\WebApplication4.csproj"
			}
		}
        
      stage('Start Sonarqube Analysis') {
			  when {
				  expression {
					  BRANCH_NAME == 'master'
				  }
			 }
        steps {
            echo "Start Sonarqube analysis"
               withSonarQubeEnv('Test_Sonar') {
                    bat "${scannerHome}\\SonarScanner.MSBuild.exe begin /k:sonar-aakashgarg /n:sonar-aakashgarg /v:1.0"
              }
            }
        }
        
        stage('Code build') {
            steps {
				// Cleans the output of a project
                echo "Clean previous build"
                bat "dotnet clean WebApplication4\\WebApplication4.csproj"
				
				// Builds the project and its dependencies
				echo "Start Building code"
				bat 'dotnet build WebApplication4\\WebApplication4.csproj -c Release -o "WebApplication4/app/build"'
            }
        }
		
		stage('Stop Sonarqube Analysis') {
			when {
				expression {
					BRANCH_NAME == 'master'
				}
			}
            steps {
                echo "Stop Sonarqube analysis"
                withSonarQubeEnv('Test_Sonar') {
                    bat "${scannerHome}\\SonarScanner.MSBuild.exe end "
                }
            }
        }
		
		stage('Release artifact') {
			when {
				expression {
					BRANCH_NAME == 'develop'
				}
			}
			steps {
				echo "Publish Code"
				bat "dotnet publish -c Release"
			}
		}
		
		stage('Docker Image') {
			steps {
				echo "Create Docker Image"
				bat "docker build -t i-${username}-${BRANCH_NAME}:${BUILD_NUMBER} --no-cache -f Dockerfile ."
				bat "docker tag i-${username}-${BRANCH_NAME}:${BUILD_NUMBER} ${registry}:${BUILD_NUMBER}"
				bat "docker tag i-${username}-${BRANCH_NAME}:${BUILD_NUMBER} ${registry}:latest"
			}
		}
		
		stage('Containers') {
			parallel {
				stage('PreContainerCheck') {
					environment {
				        CONTAINER_ID = "${bat(script:"docker ps -a --filter publish=${docker_port} --format {{.ID}}", returnStdout: true).trim().readLines().drop(1).join("")}" 
				    }
				    steps {
                        echo "Running pre container check"
				        script {
				        if(env.CONTAINER_ID != null) {
				            echo "Removing container: ${env.CONTAINER_ID}"
				            bat "docker rm -f ${env.CONTAINER_ID}"        
				        }
				   }
                                   
                    }
				}
				stage('PushtoDockerHub') {
					steps {
						echo "Push Image to Docker Hub"
				
						withDockerRegistry([credentialsId: 'DockerHub', url: ""]) {
							bat "docker push ${registry}:${BUILD_NUMBER}"
							bat "docker push ${registry}:latest"
						}
					}
				}
			}
		}
		
		stage('Docker Deployment') {
			steps {
				echo "Docker Deployment"
				bat "docker run --name c-${username}-${BRANCH_NAME} -d -p ${docker_port}:80 ${registry}:${BUILD_NUMBER}"
			}
		}
	    
	    stage('Kubernetes Deployment') {
			steps {
				echo "Deploying to Kubernetes"
				powershell "(Get-Content deployment.yaml).Replace('{{deployment}}', '${deployment_name}').Replace('{{service}}', '${service_name}').Replace('{{port}}', '${k8_port}') | set-content deployment.yaml"
				bat "kubectl apply -f deployment.yaml"
				
			}
		}
		
		
	}

        
}
